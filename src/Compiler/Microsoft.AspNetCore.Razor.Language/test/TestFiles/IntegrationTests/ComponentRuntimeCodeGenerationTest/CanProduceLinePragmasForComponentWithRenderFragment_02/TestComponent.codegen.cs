﻿// <auto-generated/>
#pragma warning disable 1591
namespace Test
{
    #line default
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Linq;
    using global::System.Threading.Tasks;
    using global::Microsoft.AspNetCore.Components;
    #line default
    #line hidden
    #nullable restore
    public partial class TestComponent : global::Microsoft.AspNetCore.Components.ComponentBase
    #nullable disable
    {
        #pragma warning disable 1998
        protected override void BuildRenderTree(global::Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
            __builder.OpenElement(0, "div");
            __builder.AddAttribute(1, "class", "row");
            __builder.OpenElement(2, "a");
            __builder.AddAttribute(3, "href", "#");
            __builder.AddAttribute(4, "@onclick", "Toggle");
            __builder.AddAttribute(5, "class", "col-12");
            __builder.AddContent(6, 
#nullable restore
#line (2,47)-(2,57) "x:\dir\subdir\Test\TestComponent.cshtml"
ActionText

#line default
#line hidden
#nullable disable
            );
            __builder.CloseElement();
#nullable restore
#line (3,4)-(5,1) "x:\dir\subdir\Test\TestComponent.cshtml"
if (!Collapsed)
  {

#line default
#line hidden
#nullable disable

            __builder.OpenElement(7, "div");
            __builder.AddAttribute(8, "class", "col-12 card card-body");
            __builder.AddContent(9, 
#nullable restore
#line (6,8)-(6,20) "x:\dir\subdir\Test\TestComponent.cshtml"
ChildContent

#line default
#line hidden
#nullable disable
            );
            __builder.CloseElement();
#nullable restore
#line (8,1)-(9,1) "x:\dir\subdir\Test\TestComponent.cshtml"
  }

#line default
#line hidden
#nullable disable

            __builder.CloseElement();
        }
        #pragma warning restore 1998
#nullable restore
#line (11,2)-(13,75) "x:\dir\subdir\Test\TestComponent.cshtml"

  [Parameter]
  public RenderFragment<string> ChildContent { get; set; } = (context) => 

#line default
#line hidden
#nullable disable

        (__builder2) => {
            __builder2.OpenElement(10, "p");
            __builder2.AddContent(11, 
#nullable restore
#line (13,80)-(13,87) "x:\dir\subdir\Test\TestComponent.cshtml"
context

#line default
#line hidden
#nullable disable
            );
            __builder2.CloseElement();
        }
#nullable restore
#line (13,91)-(21,1) "x:\dir\subdir\Test\TestComponent.cshtml"
;
  [Parameter]
  public bool Collapsed { get; set; }
  string ActionText { get => Collapsed ? "Expand" : "Collapse"; }
  void Toggle()
  {
    Collapsed = !Collapsed;
  }

#line default
#line hidden
#nullable disable

    }
}
#pragma warning restore 1591