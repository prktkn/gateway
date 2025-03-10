using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GhostNetwork.Gateway.Facade;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace GhostNetwork.Gateway.Api
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class NewsFeedController : ControllerBase
    {
        private readonly INewsFeedManager newsFeedManager;

        public NewsFeedController(INewsFeedManager newsFeedManager)
        {
            this.newsFeedManager = newsFeedManager;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerResponseHeader(StatusCodes.Status200OK, Consts.Headers.TotalCount, "Number", "")]
        [SwaggerResponseHeader(StatusCodes.Status200OK, Consts.Headers.HasMore, "String", "")]
        public async Task<ActionResult<IEnumerable<NewsFeedPublication>>> GetAsync(
            [FromServices] ICurrentUserProvider currentUserProvider,
            [FromQuery, Range(0, int.MaxValue)] int skip = 0,
            [FromQuery, Range(1, 50)] int take = 20)
        {
            var (news, totalCount) = await newsFeedManager.FindManyAsync(skip, take, currentUserProvider.UserId);

            Response.Headers.Add(Consts.Headers.TotalCount, totalCount.ToString());
            Response.Headers.Add(Consts.Headers.HasMore, (skip + take < totalCount).ToString());

            return Ok(news);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<NewsFeedPublication>> CreateAsync(
            [FromServices] ICurrentUserProvider currentUserProvider,
            [FromBody] CreateNewsFeedPublication model)
        {
            return Created(string.Empty, await newsFeedManager.CreateAsync(model.Content, currentUserProvider.UserId));
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> UpdateAsync(
            [FromRoute] string id,
            [FromBody] CreateNewsFeedPublication model)
        {
            await newsFeedManager.UpdateAsync(id, model.Content);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DeleteAsync([FromRoute] string id)
        {
            await newsFeedManager.DeleteAsync(id);

            return Ok();
        }

        [HttpPost("{publicationId}/reaction")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult> AddReactionAsync(
            [FromServices] ICurrentUserProvider currentUserProvider,
            [FromRoute] string publicationId,
            [FromBody] AddNewsFeedReaction model)
        {
            await newsFeedManager.AddReactionAsync(publicationId, currentUserProvider.UserId, model.Reaction);

            return Ok(await newsFeedManager.GetReactionsAsync(publicationId, currentUserProvider.UserId));
        }

        [HttpDelete("{publicationId}/reaction")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> RemoveReactionAsync(
            [FromServices] ICurrentUserProvider currentUserProvider,
            [FromRoute] string publicationId)
        {
            await newsFeedManager.RemoveReactionAsync(publicationId, currentUserProvider.UserId);

            return Ok(await newsFeedManager.GetReactionsAsync(publicationId, currentUserProvider.UserId));
        }

        [HttpGet("{publicationId}/comments")]
        [SwaggerResponseHeader(StatusCodes.Status200OK, Consts.Headers.TotalCount, "Number", "")]
        [SwaggerResponseHeader(StatusCodes.Status200OK, Consts.Headers.HasMore, "String", "")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> SearchCommentsAsync(
            [FromRoute] string publicationId,
            [FromQuery, Range(0, int.MaxValue)] int skip,
            [FromQuery, Range(0, 100)] int take = 10)
        {
            var (comments, totalCount) = await newsFeedManager.SearchCommentsAsync(publicationId, skip, take);

            Response.Headers.Add(Consts.Headers.TotalCount, totalCount.ToString());
            Response.Headers.Add(Consts.Headers.HasMore, (skip + take < totalCount).ToString());

            return Ok(comments);
        }

        [HttpGet("{commentId}/comment")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PublicationComment>> GetCommentByIdAsync([FromRoute] string commentId)
        {
            return Ok(await newsFeedManager.GetCommentByIdAsync(commentId));
        }

        [HttpPost("{publicationId}/comment")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<PublicationComment>> AddCommentAsync(
            [FromServices] ICurrentUserProvider currentUserProvider,
            [FromRoute] string publicationId,
            [FromBody] AddNewsFeedComment model)
        {
            await newsFeedManager.AddCommentAsync(publicationId, currentUserProvider.UserId, model.Content);

            return Ok();
        }

        [HttpDelete("{commentId}/comment")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PublicationComment>> DeleteCommentAsync([FromRoute] string commentId)
        {
            await newsFeedManager.DeleteCommentAsync(commentId);
            return Ok();
        }
    }
}
