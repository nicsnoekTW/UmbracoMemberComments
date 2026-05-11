AttachmentSearchTemplate.cshtml is an Umbraco template that uses a MultiSearcher to search across content and attachments (media). 

For media, it maps to the content page that it is attached to and reports the name of the attaching attribute (eg. "agenda", "minutes", "reports" etc).

It also checks for member permission to the content page before returning the result. 

The MultiSearcher is created as follows (file `ExamineComposer.cs`):

<pre>
using Examine;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using UmbracoExamine.PDF;
using MemberComments.Composers;
using MemberComments.Examine;

namespace MySite.MyCustomIndex;

[ComposeAfter(typeof(ExaminePdfComposer))]
[ComposeAfter(typeof(MemberCommentsComposer))]
public class ExamineComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddExamineLuceneMultiSearcher("MultiSearcher", new[] {Constants.UmbracoIndexes.ExternalIndexName, PdfIndexConstants.PdfIndexName, CommentsIndexConstants.IndexName});
    }
}

</pre>
