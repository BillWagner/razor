﻿// <auto-generated/>
#pragma warning disable 1591
namespace 
#nullable restore
#line (2,12)-(2,23) "x:\dir\subdir\Test\TestComponent.cshtml"
AnotherTest

#line default
#line hidden
#nullable disable
{
    #line default
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Linq;
    using global::System.Threading.Tasks;
    using global::Microsoft.AspNetCore.Components;
#nullable restore
#line (1,2)-(2,1) "x:\dir\subdir\Test\TestComponent.cshtml"
using Test

#line default
#line hidden
#nullable disable
    ;
    #nullable restore
    public partial class TestComponent : global::Microsoft.AspNetCore.Components.ComponentBase
    #nullable disable
    {
        #pragma warning disable 1998
        protected override void BuildRenderTree(global::Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
            __builder.OpenComponent<global::Test.HeaderComponent>(0);
            __builder.AddComponentParameter(1, nameof(global::Test.HeaderComponent.
#nullable restore
#line (4,18)-(4,24) "x:\dir\subdir\Test\TestComponent.cshtml"
Header

#line default
#line hidden
#nullable disable
            ), "head");
            __builder.CloseComponent();
            __builder.AddMarkupContent(2, "\r\n");
            __builder.OpenComponent<global::AnotherTest.FooterComponent>(3);
            __builder.AddComponentParameter(4, nameof(global::AnotherTest.FooterComponent.
#nullable restore
#line (6,18)-(6,24) "x:\dir\subdir\Test\TestComponent.cshtml"
Footer

#line default
#line hidden
#nullable disable
            ), "feet");
            __builder.CloseComponent();
        }
        #pragma warning restore 1998
    }
}
#pragma warning restore 1591
