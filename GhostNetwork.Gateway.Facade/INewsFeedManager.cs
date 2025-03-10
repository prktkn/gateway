using System.Collections.Generic;
using System.Threading.Tasks;

namespace GhostNetwork.Gateway.Facade
{
    public interface INewsFeedManager
    {
        Task<(IEnumerable<NewsFeedPublication>, long)> FindManyAsync(int skip, int take, string author);

        Task<NewsFeedPublication> CreateAsync(string content, string author);

        Task UpdateAsync(string id, string content);

        Task DeleteAsync(string id);

        Task AddCommentAsync(string publicationId, string author, string content);

        Task<PublicationComment> GetCommentByIdAsync(string id);

        Task<(IEnumerable<PublicationComment>, long)> SearchCommentsAsync(string publicationId, int skip, int take);

        Task<ReactionShort> GetReactionsAsync(string publicationId, string author);

        Task AddReactionAsync(string publicationId, string author, ReactionType type);

        Task DeleteCommentAsync(string id);

        Task RemoveReactionAsync(string publicationId, string author);
    }
}
