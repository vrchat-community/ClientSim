"use strict";(self.webpackChunkclient_sim=self.webpackChunkclient_sim||[]).push([[40],{3905:(e,t,n)=>{n.d(t,{Zo:()=>c,kt:()=>d});var s=n(7294);function r(e,t,n){return t in e?Object.defineProperty(e,t,{value:n,enumerable:!0,configurable:!0,writable:!0}):e[t]=n,e}function i(e,t){var n=Object.keys(e);if(Object.getOwnPropertySymbols){var s=Object.getOwnPropertySymbols(e);t&&(s=s.filter((function(t){return Object.getOwnPropertyDescriptor(e,t).enumerable}))),n.push.apply(n,s)}return n}function a(e){for(var t=1;t<arguments.length;t++){var n=null!=arguments[t]?arguments[t]:{};t%2?i(Object(n),!0).forEach((function(t){r(e,t,n[t])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(n)):i(Object(n)).forEach((function(t){Object.defineProperty(e,t,Object.getOwnPropertyDescriptor(n,t))}))}return e}function o(e,t){if(null==e)return{};var n,s,r=function(e,t){if(null==e)return{};var n,s,r={},i=Object.keys(e);for(s=0;s<i.length;s++)n=i[s],t.indexOf(n)>=0||(r[n]=e[n]);return r}(e,t);if(Object.getOwnPropertySymbols){var i=Object.getOwnPropertySymbols(e);for(s=0;s<i.length;s++)n=i[s],t.indexOf(n)>=0||Object.prototype.propertyIsEnumerable.call(e,n)&&(r[n]=e[n])}return r}var l=s.createContext({}),u=function(e){var t=s.useContext(l),n=t;return e&&(n="function"==typeof e?e(t):a(a({},t),e)),n},c=function(e){var t=u(e.components);return s.createElement(l.Provider,{value:t},e.children)},p="mdxType",m={inlineCode:"code",wrapper:function(e){var t=e.children;return s.createElement(s.Fragment,{},t)}},y=s.forwardRef((function(e,t){var n=e.components,r=e.mdxType,i=e.originalType,l=e.parentName,c=o(e,["components","mdxType","originalType","parentName"]),p=u(n),y=r,d=p["".concat(l,".").concat(y)]||p[y]||m[y]||i;return n?s.createElement(d,a(a({ref:t},c),{},{components:n})):s.createElement(d,a({ref:t},c))}));function d(e,t){var n=arguments,r=t&&t.mdxType;if("string"==typeof e||r){var i=n.length,a=new Array(i);a[0]=y;var o={};for(var l in t)hasOwnProperty.call(t,l)&&(o[l]=t[l]);o.originalType=e,o[p]="string"==typeof e?e:r,a[1]=o;for(var u=2;u<i;u++)a[u]=n[u];return s.createElement.apply(null,a)}return s.createElement.apply(null,n)}y.displayName="MDXCreateElement"},1276:(e,t,n)=>{n.r(t),n.d(t,{assets:()=>l,contentTitle:()=>a,default:()=>m,frontMatter:()=>i,metadata:()=>o,toc:()=>u});var s=n(7462),r=(n(7294),n(3905));const i={id:"unity-event-system",title:"Unity Event System",hide_title:!0},a="Unity Event System",o={unversionedId:"systems/runtime/unity-event-system",id:"systems/runtime/unity-event-system",title:"Unity Event System",description:"ClientSim uses two classes to translate actions into Unity\u2019s EventSystem. These classes decouple Unity\u2019s old input system into values based on ClientSim\u2019s current bindings and match VRChat\u2019s interactive UI object filtering.",source:"@site/docs/systems/runtime/unity-event-system.md",sourceDirName:"systems/runtime",slug:"/systems/runtime/unity-event-system",permalink:"/systems/runtime/unity-event-system",draft:!1,editUrl:"https://github.com/vrchat-community/ClientSim/edit/main/Docs/Source/systems/runtime/unity-event-system.md",tags:[],version:"current",frontMatter:{id:"unity-event-system",title:"Unity Event System",hide_title:!0},sidebar:"tutorialSidebar",previous:{title:"UdonManager",permalink:"/systems/runtime/udon-manager"},next:{title:"Script Execution Order",permalink:"/systems/script-execution-order"}},l={},u=[{value:"BaseInput",id:"baseinput",level:2},{value:"InputModule",id:"inputmodule",level:2}],c={toc:u},p="wrapper";function m(e){let{components:t,...n}=e;return(0,r.kt)(p,(0,s.Z)({},c,n,{components:t,mdxType:"MDXLayout"}),(0,r.kt)("h1",{id:"unity-event-system"},"Unity Event System"),(0,r.kt)("p",null,"ClientSim uses two classes to translate actions into Unity\u2019s EventSystem. These classes decouple Unity\u2019s old input system into values based on ClientSim\u2019s current bindings and match VRChat\u2019s interactive UI object filtering. "),(0,r.kt)("h2",{id:"baseinput"},"BaseInput"),(0,r.kt)("p",null,"The ClientSimBaseInput system extends Unity\u2019s BaseInput class. Unity\u2019s BaseInput is responsible for passing mouse position and button input into the EventSystem. The ClientSim BaseInput system overrides these methods to instead pass values based on the current ClientSim input bindings and last ",(0,r.kt)("a",{parentName:"p",href:"/systems/runtime/player#raycaster"},"PlayerRaycaster")," results. Mouse input is replaced with the current binding\u2019s ",(0,r.kt)("a",{parentName:"p",href:"/systems/runtime/input"},"Use Input"),". Since Use input is a handed action, only the value of the last activated hand is passed as mouse input. The mouse position sent to the Event System ignores the actual mouse position, and instead calculates the screen position of the last interact raycast. Using the raycast position abstracts out the real mouse\u2019s position, allowing Desktop and VR to use Unity UI through the same system.\nThe BaseInput system is also responsible for providing the current mouse position to the rest of ClientSim. It controls if the mouse pointer is hidden and locked to the center of the screen, or visible and free to move. This mouse position is used for displaying the ",(0,r.kt)("a",{parentName:"p",href:"/systems/runtime/player#reticle"},"Desktop Reticle")," as well as using the mouse to create the ray direction for ",(0,r.kt)("a",{parentName:"p",href:"/systems/runtime/player#rayprovider"},"DesktopRayProvider"),"."),(0,r.kt)("h2",{id:"inputmodule"},"InputModule"),(0,r.kt)("p",null,"The ClientSimInputModule extends Unity\u2019s StandaloneInputModule. This system processes Unity mouse events and filters out any UI objects that are not currently interactable. UI objects are interactable when all of the following conditions have been met:"),(0,r.kt)("ol",null,(0,r.kt)("li",{parentName:"ol"},"The ",(0,r.kt)("a",{parentName:"li",href:"/systems/runtime/player#playerraycaster"},"PlayerRaycaster")," last hit an object with a VRC_UIShape component. This data is provided through ClientSimBaseInput."),(0,r.kt)("li",{parentName:"ol"},"The UI object has a UIShape component in its parent"),(0,r.kt)("li",{parentName:"ol"},"The layer of the parent UIShape object is on a currently interactive layer. Interactive layers are determined by the ",(0,r.kt)("a",{parentName:"li",href:"/systems/runtime/interactive-layer-provider"},"InteractiveLayerProvider"),"."),(0,r.kt)("li",{parentName:"ol"},"The hit point of the UI Object raycast is contained within the collider of the UIShape. If any of those conditions fail, then the UI cannot be interacted with.")))}m.isMDXComponent=!0}}]);