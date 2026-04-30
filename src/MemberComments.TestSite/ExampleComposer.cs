using Examine;
using MemberComments.Composers;
using MemberComments.Examine;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;

namespace MySite.MyCustomIndex;

[ComposeAfter(typeof(MemberCommentsComposer))]
public class ExamineComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddExamineLuceneMultiSearcher("MultiSearcher", new[] {Constants.UmbracoIndexes.ExternalIndexName, CommentsIndexConstants.IndexName});
    }
}