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
#line (1,12)-(1,18) "x:\dir\subdir\Test\TestComponent.cshtml"
TParam

#line default
#line hidden
#nullable disable
    > : global::Microsoft.AspNetCore.Components.ComponentBase
    #nullable disable
    {
        #pragma warning disable 1998
        protected override void BuildRenderTree(global::Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
            global::__Blazor.Test.TestComponent.TypeInference.CreateMyComponent_0(__builder, 0, 1, 
#nullable restore
#line (2,31)-(2,42) "x:\dir\subdir\Test\TestComponent.cshtml"
ParentValue

#line default
#line hidden
#nullable disable
            , 2, global::Microsoft.AspNetCore.Components.EventCallback.Factory.Create(this, global::Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.CreateInferredEventCallback(this, 
#nullable restore
#line (2,61)-(2,72) "x:\dir\subdir\Test\TestComponent.cshtml"
UpdateValue

#line default
#line hidden
#nullable disable
            , ParentValue)));
        }
        #pragma warning restore 1998
#nullable restore
#line (3,8)-(7,1) "x:\dir\subdir\Test\TestComponent.cshtml"

    public TParam ParentValue { get; set; } = default;

    public void UpdateValue(TParam value) { ParentValue = value; }

#line default
#line hidden
#nullable disable

    }
}
namespace __Blazor.Test.TestComponent
{
    #line hidden
    internal static class TypeInference
    {
        public static void CreateMyComponent_0<TValue>(global::Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder, int seq, int __seq0, TValue __arg0, int __seq1, global::Microsoft.AspNetCore.Components.EventCallback<TValue> __arg1)
        {
        __builder.OpenComponent<global::Test.MyComponent<TValue>>(seq);
        __builder.AddComponentParameter(__seq0, nameof(global::Test.MyComponent<TValue>.
#nullable restore
#line (2,20)-(2,25) "x:\dir\subdir\Test\TestComponent.cshtml"
Value

#line default
#line hidden
#nullable disable
        ), __arg0);
        __builder.AddComponentParameter(__seq1, nameof(global::Test.MyComponent<TValue>.ValueChanged), __arg1);
        __builder.CloseComponent();
        }
    }
}
#pragma warning restore 1591
