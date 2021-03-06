using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MvcTemplate.Components.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MvcTemplate.Components.Mvc
{
    [HtmlTargetElement("div", Attributes = "mvc-tree-for")]
    public class MvcTreeTagHelper : TagHelper
    {
        [HtmlAttributeName("readonly")]
        public Boolean Readonly { get; set; }

        [HtmlAttributeName("hide-depth")]
        public Int32? HideDepth { get; set; }

        [HtmlAttributeName("mvc-tree-for")]
        public ModelExpression? For { get; set; }

        public override void Process(TagHelperContext? context, TagHelperOutput output)
        {
            String treeClasses = "mvc-tree";
            MvcTree tree = For?.Model as MvcTree ?? new MvcTree();

            if (Readonly)
                treeClasses += " mvc-tree-readonly";

            output.Content.AppendHtml(IdsFor(tree));
            output.Content.AppendHtml(ViewFor(tree));

            output.Attributes.SetAttribute("data-for", $"{For?.Name}.SelectedIds");
            output.Attributes.SetAttribute("class", $"{treeClasses} {output.Attributes["class"]?.Value}".Trim());
        }

        private TagBuilder IdsFor(MvcTree model)
        {
            String name = $"{For?.Name}.SelectedIds";
            TagBuilder ids = new TagBuilder("div");
            ids.AddCssClass("mvc-tree-ids");

            foreach (Int64 id in model.SelectedIds)
            {
                TagBuilder input = new TagBuilder("input");
                input.TagRenderMode = TagRenderMode.SelfClosing;
                input.Attributes["value"] = id.ToString();
                input.Attributes["type"] = "hidden";
                input.Attributes["name"] = name;

                ids.InnerHtml.AppendHtml(input);
            }

            return ids;
        }
        private TagBuilder ViewFor(MvcTree model)
        {
            TagBuilder view = new TagBuilder("ul");
            view.AddCssClass("mvc-tree-view");

            foreach (TagBuilder node in Build(model, model.Nodes, 1))
                view.InnerHtml.AppendHtml(node);

            return view;
        }
        private IEnumerable<TagBuilder> Build(MvcTree model, List<MvcTreeNode> nodes, Int32 depth)
        {
            return nodes.Select(node =>
            {
                TagBuilder item = new TagBuilder("li");
                item.InnerHtml.AppendHtml("<i></i>");

                if (node.Id is Int64 id)
                {
                    if (model.SelectedIds.Contains(id))
                        item.AddCssClass("mvc-tree-checked");

                    item.Attributes["data-id"] = id.ToString();
                }

                TagBuilder anchor = new TagBuilder("a");
                anchor.InnerHtml.Append(node.Title);
                anchor.Attributes["href"] = "#";

                item.InnerHtml.AppendHtml(anchor);

                if (node.Children.Count > 0)
                {
                    item.AddCssClass("mvc-tree-branch");

                    if (HideDepth <= depth)
                        item.AddCssClass("mvc-tree-collapsed");

                    TagBuilder branch = new TagBuilder("ul");

                    foreach (TagBuilder leaf in Build(model, node.Children, depth + 1))
                        branch.InnerHtml.AppendHtml(leaf);

                    item.InnerHtml.AppendHtml(branch);
                }

                return item;
            });
        }
    }
}
