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
    public partial class TestComponent<
#nullable restore
#line (1,12)-(1,19) "x:\dir\subdir\Test\TestComponent.cshtml"
TDomain

#line default
#line hidden
#nullable disable
    ,
#nullable restore
#line (2,12)-(2,18) "x:\dir\subdir\Test\TestComponent.cshtml"
TValue

#line default
#line hidden
#nullable disable
    > : global::Microsoft.AspNetCore.Components.ComponentBase
#nullable restore
#line (1,20)-(1,42) "x:\dir\subdir\Test\TestComponent.cshtml"
where TDomain : struct

#line default
#line hidden
#nullable disable
#nullable restore
#line (2,19)-(2,40) "x:\dir\subdir\Test\TestComponent.cshtml"
where TValue : struct

#line default
#line hidden
#nullable disable
    #nullable disable
    {
        #pragma warning disable 1998
        protected override void BuildRenderTree(global::Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
            __builder.OpenComponent<global::Test.TestComponent<decimal, decimal>>(0);
            __builder.AddComponentParameter(1, nameof(global::Test.TestComponent<decimal, decimal>.
#nullable restore
#line (4,16)-(4,20) "x:\dir\subdir\Test\TestComponent.cshtml"
Data

#line default
#line hidden
#nullable disable
            ), global::Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<global::System.Collections.Generic.List<(decimal Domain, decimal Value)>>(
#nullable restore
#line (4,22)-(4,26) "x:\dir\subdir\Test\TestComponent.cshtml"
null

#line default
#line hidden
#nullable disable
            ));
            __builder.CloseComponent();
        }
        #pragma warning restore 1998
#nullable restore
#line (6,8)-(9,1) "x:\dir\subdir\Test\TestComponent.cshtml"

    [Parameter]
    public List<(TDomain Domain, TValue Value)> Data { get; set; }

#line default
#line hidden
#nullable disable

    }
}
#pragma warning restore 1591
