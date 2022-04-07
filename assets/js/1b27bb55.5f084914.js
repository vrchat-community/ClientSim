"use strict";(self.webpackChunkclient_sim=self.webpackChunkclient_sim||[]).push([[6483],{3905:function(e,t,n){n.d(t,{Zo:function(){return c},kt:function(){return p}});var r=n(7294);function i(e,t,n){return t in e?Object.defineProperty(e,t,{value:n,enumerable:!0,configurable:!0,writable:!0}):e[t]=n,e}function o(e,t){var n=Object.keys(e);if(Object.getOwnPropertySymbols){var r=Object.getOwnPropertySymbols(e);t&&(r=r.filter((function(t){return Object.getOwnPropertyDescriptor(e,t).enumerable}))),n.push.apply(n,r)}return n}function s(e){for(var t=1;t<arguments.length;t++){var n=null!=arguments[t]?arguments[t]:{};t%2?o(Object(n),!0).forEach((function(t){i(e,t,n[t])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(n)):o(Object(n)).forEach((function(t){Object.defineProperty(e,t,Object.getOwnPropertyDescriptor(n,t))}))}return e}function a(e,t){if(null==e)return{};var n,r,i=function(e,t){if(null==e)return{};var n,r,i={},o=Object.keys(e);for(r=0;r<o.length;r++)n=o[r],t.indexOf(n)>=0||(i[n]=e[n]);return i}(e,t);if(Object.getOwnPropertySymbols){var o=Object.getOwnPropertySymbols(e);for(r=0;r<o.length;r++)n=o[r],t.indexOf(n)>=0||Object.prototype.propertyIsEnumerable.call(e,n)&&(i[n]=e[n])}return i}var l=r.createContext({}),u=function(e){var t=r.useContext(l),n=t;return e&&(n="function"==typeof e?e(t):s(s({},t),e)),n},c=function(e){var t=u(e.components);return r.createElement(l.Provider,{value:t},e.children)},m={inlineCode:"code",wrapper:function(e){var t=e.children;return r.createElement(r.Fragment,{},t)}},d=r.forwardRef((function(e,t){var n=e.components,i=e.mdxType,o=e.originalType,l=e.parentName,c=a(e,["components","mdxType","originalType","parentName"]),d=u(n),p=i,f=d["".concat(l,".").concat(p)]||d[p]||m[p]||o;return n?r.createElement(f,s(s({ref:t},c),{},{components:n})):r.createElement(f,s({ref:t},c))}));function p(e,t){var n=arguments,i=t&&t.mdxType;if("string"==typeof e||i){var o=n.length,s=new Array(o);s[0]=d;var a={};for(var l in t)hasOwnProperty.call(t,l)&&(a[l]=t[l]);a.originalType=e,a.mdxType="string"==typeof e?e:i,s[1]=a;for(var u=2;u<o;u++)s[u]=n[u];return r.createElement.apply(null,s)}return r.createElement.apply(null,n)}d.displayName="MDXCreateElement"},3136:function(e,t,n){n.r(t),n.d(t,{assets:function(){return c},contentTitle:function(){return l},default:function(){return p},frontMatter:function(){return a},metadata:function(){return u},toc:function(){return m}});var r=n(7462),i=n(3366),o=(n(7294),n(3905)),s=["components"],a={id:"editor-runtime-linker",title:"Editor Runtime Linker",hide_title:!0},l="Editor Runtime Linker",u={unversionedId:"systems/editor/editor-runtime-linker",id:"systems/editor/editor-runtime-linker",title:"Editor Runtime Linker",description:"This system links and unlinks, on enter and exit playmode, the Editor only hooks in the ClientSim Menu for checking if settings are invalid and a method to open the ClientSim Settings Window.",source:"@site/docs/systems/editor/editor-runtime-linker.md",sourceDirName:"systems/editor",slug:"/systems/editor/editor-runtime-linker",permalink:"/ClientSim/systems/editor/editor-runtime-linker",tags:[],version:"current",frontMatter:{id:"editor-runtime-linker",title:"Editor Runtime Linker",hide_title:!0},sidebar:"tutorialSidebar",previous:{title:"Editor",permalink:"/ClientSim/systems/editor/"},next:{title:"Helper Editors",permalink:"/ClientSim/systems/editor/helper-editors"}},c={},m=[],d={toc:m};function p(e){var t=e.components,n=(0,i.Z)(e,s);return(0,o.kt)("wrapper",(0,r.Z)({},d,n,{components:t,mdxType:"MDXLayout"}),(0,o.kt)("h1",{id:"editor-runtime-linker"},"Editor Runtime Linker"),(0,o.kt)("p",null,"This system links and unlinks, on enter and exit playmode, the Editor only hooks in the ",(0,o.kt)("a",{parentName:"p",href:"/ClientSim/systems/runtime/menu"},"ClientSim Menu")," for checking if settings are invalid and a method to open the ",(0,o.kt)("a",{parentName:"p",href:"/ClientSim/systems/editor/settings-window"},"ClientSim Settings Window"),"."))}p.isMDXComponent=!0}}]);