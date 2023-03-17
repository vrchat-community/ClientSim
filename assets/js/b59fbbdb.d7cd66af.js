"use strict";(self.webpackChunkclient_sim=self.webpackChunkclient_sim||[]).push([[7177],{3905:(e,r,t)=>{t.d(r,{Zo:()=>u,kt:()=>d});var n=t(7294);function i(e,r,t){return r in e?Object.defineProperty(e,r,{value:t,enumerable:!0,configurable:!0,writable:!0}):e[r]=t,e}function a(e,r){var t=Object.keys(e);if(Object.getOwnPropertySymbols){var n=Object.getOwnPropertySymbols(e);r&&(n=n.filter((function(r){return Object.getOwnPropertyDescriptor(e,r).enumerable}))),t.push.apply(t,n)}return t}function o(e){for(var r=1;r<arguments.length;r++){var t=null!=arguments[r]?arguments[r]:{};r%2?a(Object(t),!0).forEach((function(r){i(e,r,t[r])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(t)):a(Object(t)).forEach((function(r){Object.defineProperty(e,r,Object.getOwnPropertyDescriptor(t,r))}))}return e}function s(e,r){if(null==e)return{};var t,n,i=function(e,r){if(null==e)return{};var t,n,i={},a=Object.keys(e);for(n=0;n<a.length;n++)t=a[n],r.indexOf(t)>=0||(i[t]=e[t]);return i}(e,r);if(Object.getOwnPropertySymbols){var a=Object.getOwnPropertySymbols(e);for(n=0;n<a.length;n++)t=a[n],r.indexOf(t)>=0||Object.prototype.propertyIsEnumerable.call(e,t)&&(i[t]=e[t])}return i}var c=n.createContext({}),l=function(e){var r=n.useContext(c),t=r;return e&&(t="function"==typeof e?e(r):o(o({},r),e)),t},u=function(e){var r=l(e.components);return n.createElement(c.Provider,{value:r},e.children)},p="mdxType",y={inlineCode:"code",wrapper:function(e){var r=e.children;return n.createElement(n.Fragment,{},r)}},m=n.forwardRef((function(e,r){var t=e.components,i=e.mdxType,a=e.originalType,c=e.parentName,u=s(e,["components","mdxType","originalType","parentName"]),p=l(t),m=i,d=p["".concat(c,".").concat(m)]||p[m]||y[m]||a;return t?n.createElement(d,o(o({ref:r},u),{},{components:t})):n.createElement(d,o({ref:r},u))}));function d(e,r){var t=arguments,i=r&&r.mdxType;if("string"==typeof e||i){var a=t.length,o=new Array(a);o[0]=m;var s={};for(var c in r)hasOwnProperty.call(r,c)&&(s[c]=r[c]);s.originalType=e,s[p]="string"==typeof e?e:i,o[1]=s;for(var l=2;l<a;l++)o[l]=t[l];return n.createElement.apply(null,o)}return n.createElement.apply(null,t)}m.displayName="MDXCreateElement"},1306:(e,r,t)=>{t.r(r),t.d(r,{assets:()=>c,contentTitle:()=>o,default:()=>y,frontMatter:()=>a,metadata:()=>s,toc:()=>l});var n=t(7462),i=(t(7294),t(3905));const a={id:"interactive-layer-provider",title:"InteractiveLayerProvider",hide_title:!0},o="InteractiveLayerProvider",s={unversionedId:"systems/runtime/interactive-layer-provider",id:"systems/runtime/interactive-layer-provider",title:"InteractiveLayerProvider",description:"The InteractiveLayerProvider simply listens to menu open state events and provides a layer mask for which layers are currently interactive. When the menu is open, only the UI and UIMenu layers are interactive. When the menu is closed, all other layers, excluding MirrorReflection, are interactive. InteractiveLayerProvider is used by Raycasters and the ClientSimInputModule.",source:"@site/docs/systems/runtime/interactive-layer-provider.md",sourceDirName:"systems/runtime",slug:"/systems/runtime/interactive-layer-provider",permalink:"/systems/runtime/interactive-layer-provider",draft:!1,editUrl:"https://github.com/vrchat-community/ClientSim/edit/main/Docs/Source/systems/runtime/interactive-layer-provider.md",tags:[],version:"current",frontMatter:{id:"interactive-layer-provider",title:"InteractiveLayerProvider",hide_title:!0},sidebar:"tutorialSidebar",previous:{title:"Input",permalink:"/systems/runtime/input"},next:{title:"Client Sim Main",permalink:"/systems/runtime/main"}},c={},l=[],u={toc:l},p="wrapper";function y(e){let{components:r,...t}=e;return(0,i.kt)(p,(0,n.Z)({},u,t,{components:r,mdxType:"MDXLayout"}),(0,i.kt)("h1",{id:"interactivelayerprovider"},"InteractiveLayerProvider"),(0,i.kt)("p",null,"The InteractiveLayerProvider simply listens to menu open state events and provides a layer mask for which layers are currently interactive. When the menu is open, only the UI and UIMenu layers are interactive. When the menu is closed, all other layers, excluding MirrorReflection, are interactive. InteractiveLayerProvider is used by ",(0,i.kt)("a",{parentName:"p",href:"/systems/runtime/player#raycaster"},"Raycasters")," and the ",(0,i.kt)("a",{parentName:"p",href:"/systems/runtime/input"},"ClientSimInputModule"),"."))}y.isMDXComponent=!0}}]);