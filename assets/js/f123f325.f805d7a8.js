"use strict";(self.webpackChunkclient_sim=self.webpackChunkclient_sim||[]).push([[2003],{3905:(e,t,n)=>{n.d(t,{Zo:()=>c,kt:()=>f});var i=n(7294);function r(e,t,n){return t in e?Object.defineProperty(e,t,{value:n,enumerable:!0,configurable:!0,writable:!0}):e[t]=n,e}function a(e,t){var n=Object.keys(e);if(Object.getOwnPropertySymbols){var i=Object.getOwnPropertySymbols(e);t&&(i=i.filter((function(t){return Object.getOwnPropertyDescriptor(e,t).enumerable}))),n.push.apply(n,i)}return n}function o(e){for(var t=1;t<arguments.length;t++){var n=null!=arguments[t]?arguments[t]:{};t%2?a(Object(n),!0).forEach((function(t){r(e,t,n[t])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(n)):a(Object(n)).forEach((function(t){Object.defineProperty(e,t,Object.getOwnPropertyDescriptor(n,t))}))}return e}function s(e,t){if(null==e)return{};var n,i,r=function(e,t){if(null==e)return{};var n,i,r={},a=Object.keys(e);for(i=0;i<a.length;i++)n=a[i],t.indexOf(n)>=0||(r[n]=e[n]);return r}(e,t);if(Object.getOwnPropertySymbols){var a=Object.getOwnPropertySymbols(e);for(i=0;i<a.length;i++)n=a[i],t.indexOf(n)>=0||Object.prototype.propertyIsEnumerable.call(e,n)&&(r[n]=e[n])}return r}var l=i.createContext({}),m=function(e){var t=i.useContext(l),n=t;return e&&(n="function"==typeof e?e(t):o(o({},t),e)),n},c=function(e){var t=m(e.components);return i.createElement(l.Provider,{value:t},e.children)},p="mdxType",u={inlineCode:"code",wrapper:function(e){var t=e.children;return i.createElement(i.Fragment,{},t)}},d=i.forwardRef((function(e,t){var n=e.components,r=e.mdxType,a=e.originalType,l=e.parentName,c=s(e,["components","mdxType","originalType","parentName"]),p=m(n),d=r,f=p["".concat(l,".").concat(d)]||p[d]||u[d]||a;return n?i.createElement(f,o(o({ref:t},c),{},{components:n})):i.createElement(f,o({ref:t},c))}));function f(e,t){var n=arguments,r=t&&t.mdxType;if("string"==typeof e||r){var a=n.length,o=new Array(a);o[0]=d;var s={};for(var l in t)hasOwnProperty.call(t,l)&&(s[l]=t[l]);s.originalType=e,s[p]="string"==typeof e?e:r,o[1]=s;for(var m=2;m<a;m++)o[m]=n[m];return i.createElement.apply(null,o)}return i.createElement.apply(null,n)}d.displayName="MDXCreateElement"},7689:(e,t,n)=>{n.r(t),n.d(t,{assets:()=>l,contentTitle:()=>o,default:()=>u,frontMatter:()=>a,metadata:()=>s,toc:()=>m});var i=n(7462),r=(n(7294),n(3905));const a={id:"main",title:"Client Sim Main",hide_title:!0},o="Client Sim Main",s={unversionedId:"systems/runtime/main",id:"systems/runtime/main",title:"Client Sim Main",description:"ClientSimMain is the central point of ClientSim that handles initialization and destruction of ClientSim. It is contained in the ClientSimSystem prefab. On creation, all core systems will be initialized with their dependencies. This system also maintains all the implementations of the VRCSDK hooks to link VRC components to the ClientSim Helpers. ClientSimMain is a singleton to ensure only one instance is running at a time and to help easily pass information from Editor Windows and Tests. None of the runtime systems within ClientSim depend on ClientSimMain being a singleton.",source:"@site/docs/systems/runtime/main.md",sourceDirName:"systems/runtime",slug:"/systems/runtime/main",permalink:"/systems/runtime/main",draft:!1,editUrl:"https://github.com/vrchat-community/ClientSim/edit/main/Docs/Source/systems/runtime/main.md",tags:[],version:"current",frontMatter:{id:"main",title:"Client Sim Main",hide_title:!0},sidebar:"tutorialSidebar",previous:{title:"InteractiveLayerProvider",permalink:"/systems/runtime/interactive-layer-provider"},next:{title:"Client Sim Menu",permalink:"/systems/runtime/menu"}},l={},m=[],c={toc:m},p="wrapper";function u(e){let{components:t,...a}=e;return(0,r.kt)(p,(0,i.Z)({},c,a,{components:t,mdxType:"MDXLayout"}),(0,r.kt)("h1",{id:"client-sim-main"},"Client Sim Main"),(0,r.kt)("p",null,"ClientSimMain is the central point of ClientSim that handles initialization and destruction of ClientSim. It is contained in the ClientSimSystem prefab. On creation, all core systems will be initialized with their dependencies. This system also maintains all the implementations of the VRCSDK hooks to link VRC components to the ClientSim Helpers. ClientSimMain is a singleton to ensure only one instance is running at a time and to help easily pass information from Editor Windows and Tests. None of the runtime systems within ClientSim depend on ClientSimMain being a singleton."),(0,r.kt)("p",null,(0,r.kt)("img",{alt:"ClientSimSystem Hierarchy",src:n(3532).Z,width:"658",height:"425"})))}u.isMDXComponent=!0},3532:(e,t,n)=>{n.d(t,{Z:()=>i});const i=n.p+"assets/images/client-sim-main-hierarchy-a7f31e3143d4b58ffc8c73e9eb04fbcf.png"}}]);