"use strict";(self.webpackChunkclient_sim=self.webpackChunkclient_sim||[]).push([[126],{3905:function(e,t,n){n.d(t,{Zo:function(){return c},kt:function(){return d}});var r=n(7294);function i(e,t,n){return t in e?Object.defineProperty(e,t,{value:n,enumerable:!0,configurable:!0,writable:!0}):e[t]=n,e}function o(e,t){var n=Object.keys(e);if(Object.getOwnPropertySymbols){var r=Object.getOwnPropertySymbols(e);t&&(r=r.filter((function(t){return Object.getOwnPropertyDescriptor(e,t).enumerable}))),n.push.apply(n,r)}return n}function a(e){for(var t=1;t<arguments.length;t++){var n=null!=arguments[t]?arguments[t]:{};t%2?o(Object(n),!0).forEach((function(t){i(e,t,n[t])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(n)):o(Object(n)).forEach((function(t){Object.defineProperty(e,t,Object.getOwnPropertyDescriptor(n,t))}))}return e}function l(e,t){if(null==e)return{};var n,r,i=function(e,t){if(null==e)return{};var n,r,i={},o=Object.keys(e);for(r=0;r<o.length;r++)n=o[r],t.indexOf(n)>=0||(i[n]=e[n]);return i}(e,t);if(Object.getOwnPropertySymbols){var o=Object.getOwnPropertySymbols(e);for(r=0;r<o.length;r++)n=o[r],t.indexOf(n)>=0||Object.prototype.propertyIsEnumerable.call(e,n)&&(i[n]=e[n])}return i}var s=r.createContext({}),p=function(e){var t=r.useContext(s),n=t;return e&&(n="function"==typeof e?e(t):a(a({},t),e)),n},c=function(e){var t=p(e.components);return r.createElement(s.Provider,{value:t},e.children)},u={inlineCode:"code",wrapper:function(e){var t=e.children;return r.createElement(r.Fragment,{},t)}},m=r.forwardRef((function(e,t){var n=e.components,i=e.mdxType,o=e.originalType,s=e.parentName,c=l(e,["components","mdxType","originalType","parentName"]),m=p(n),d=i,f=m["".concat(s,".").concat(d)]||m[d]||u[d]||o;return n?r.createElement(f,a(a({ref:t},c),{},{components:n})):r.createElement(f,a({ref:t},c))}));function d(e,t){var n=arguments,i=t&&t.mdxType;if("string"==typeof e||i){var o=n.length,a=new Array(o);a[0]=m;var l={};for(var s in t)hasOwnProperty.call(t,s)&&(l[s]=t[s]);l.originalType=e,l.mdxType="string"==typeof e?e:i,a[1]=l;for(var p=2;p<o;p++)a[p]=n[p];return r.createElement.apply(null,a)}return r.createElement.apply(null,n)}m.displayName="MDXCreateElement"},1425:function(e,t,n){n.r(t),n.d(t,{assets:function(){return c},contentTitle:function(){return s},default:function(){return d},frontMatter:function(){return l},metadata:function(){return p},toc:function(){return u}});var r=n(7462),i=n(3366),o=(n(7294),n(3905)),a=["components"],l={id:"tooltip-manager",title:"TooltipManager",hide_title:!0},s="TooltipManager",p={unversionedId:"systems/runtime/tooltip-manager",id:"systems/runtime/tooltip-manager",title:"TooltipManager",description:"The TooltipManager will display text in the world above a given interactable object. Tooltips in ClientSim only display text, unlike VRChat which also displays an icon of the respective button needed to use the interact. In SDK3, it appears that the option to set a tooltip location for an interactable is ignored. Tooltips always display at the top center of the first collider on the interactable object. There is no set limit to the number of tooltips that can be displayed, but only 2 tooltips are expected through ClientSim, one per player hand. Displaying tooltips can be disabled in the ClientSimSettings.",source:"@site/docs/systems/runtime/tooltip-manager.md",sourceDirName:"systems/runtime",slug:"/systems/runtime/tooltip-manager",permalink:"/ClientSim/systems/runtime/tooltip-manager",tags:[],version:"current",frontMatter:{id:"tooltip-manager",title:"TooltipManager",hide_title:!0},sidebar:"tutorialSidebar",previous:{title:"SyncedObjectManager",permalink:"/ClientSim/systems/runtime/synced-object-manager"},next:{title:"UdonManager",permalink:"/ClientSim/systems/runtime/udon-manager"}},c={},u=[],m={toc:u};function d(e){var t=e.components,n=(0,i.Z)(e,a);return(0,o.kt)("wrapper",(0,r.Z)({},m,n,{components:t,mdxType:"MDXLayout"}),(0,o.kt)("h1",{id:"tooltipmanager"},"TooltipManager"),(0,o.kt)("p",null,"The TooltipManager will display text in the world above a given interactable object. Tooltips in ClientSim only display text, unlike VRChat which also displays an icon of the respective button needed to use the interact. In SDK3, it appears that the option to set a tooltip location for an interactable is ignored. Tooltips always display at the top center of the first collider on the interactable object. There is no set limit to the number of tooltips that can be displayed, but only 2 tooltips are expected through ClientSim, one per player hand. Displaying tooltips can be disabled in the ",(0,o.kt)("a",{parentName:"p",href:"/ClientSim/systems/runtime/settings"},"ClientSimSettings"),"."))}d.isMDXComponent=!0}}]);