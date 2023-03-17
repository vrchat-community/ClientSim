"use strict";(self.webpackChunkclient_sim=self.webpackChunkclient_sim||[]).push([[9186],{3905:(e,t,n)=>{n.d(t,{Zo:()=>l,kt:()=>h});var i=n(7294);function r(e,t,n){return t in e?Object.defineProperty(e,t,{value:n,enumerable:!0,configurable:!0,writable:!0}):e[t]=n,e}function s(e,t){var n=Object.keys(e);if(Object.getOwnPropertySymbols){var i=Object.getOwnPropertySymbols(e);t&&(i=i.filter((function(t){return Object.getOwnPropertyDescriptor(e,t).enumerable}))),n.push.apply(n,i)}return n}function a(e){for(var t=1;t<arguments.length;t++){var n=null!=arguments[t]?arguments[t]:{};t%2?s(Object(n),!0).forEach((function(t){r(e,t,n[t])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(n)):s(Object(n)).forEach((function(t){Object.defineProperty(e,t,Object.getOwnPropertyDescriptor(n,t))}))}return e}function o(e,t){if(null==e)return{};var n,i,r=function(e,t){if(null==e)return{};var n,i,r={},s=Object.keys(e);for(i=0;i<s.length;i++)n=s[i],t.indexOf(n)>=0||(r[n]=e[n]);return r}(e,t);if(Object.getOwnPropertySymbols){var s=Object.getOwnPropertySymbols(e);for(i=0;i<s.length;i++)n=s[i],t.indexOf(n)>=0||Object.prototype.propertyIsEnumerable.call(e,n)&&(r[n]=e[n])}return r}var p=i.createContext({}),u=function(e){var t=i.useContext(p),n=t;return e&&(n="function"==typeof e?e(t):a(a({},t),e)),n},l=function(e){var t=u(e.components);return i.createElement(p.Provider,{value:t},e.children)},c="mdxType",d={inlineCode:"code",wrapper:function(e){var t=e.children;return i.createElement(i.Fragment,{},t)}},m=i.forwardRef((function(e,t){var n=e.components,r=e.mdxType,s=e.originalType,p=e.parentName,l=o(e,["components","mdxType","originalType","parentName"]),c=u(n),m=r,h=c["".concat(p,".").concat(m)]||c[m]||d[m]||s;return n?i.createElement(h,a(a({ref:t},l),{},{components:n})):i.createElement(h,a({ref:t},l))}));function h(e,t){var n=arguments,r=t&&t.mdxType;if("string"==typeof e||r){var s=n.length,a=new Array(s);a[0]=m;var o={};for(var p in t)hasOwnProperty.call(t,p)&&(o[p]=t[p]);o.originalType=e,o[c]="string"==typeof e?e:r,a[1]=o;for(var u=2;u<s;u++)a[u]=n[u];return i.createElement.apply(null,a)}return i.createElement.apply(null,n)}m.displayName="MDXCreateElement"},9214:(e,t,n)=>{n.r(t),n.d(t,{assets:()=>p,contentTitle:()=>a,default:()=>d,frontMatter:()=>s,metadata:()=>o,toc:()=>u});var i=n(7462),r=(n(7294),n(3905));const s={id:"input",title:"Input",hide_title:!0},a="Input",o={unversionedId:"systems/runtime/input",id:"systems/runtime/input",title:"Input",description:"In ClientSim, all input calls are in one class to handle input and send events. The ClientSimInputManager uses the new Input System, allowing for event-driven input. It uses the PlayerInput component to gain access to the specific input events based on the Input Bindings displayed below. Since the new Unity Input System package is not included by default, and Unity requires a special setting to enable, all references to the Input System are wrapped in define conditions, which prevents errors when importing into new projects.",source:"@site/docs/systems/runtime/input.md",sourceDirName:"systems/runtime",slug:"/systems/runtime/input",permalink:"/systems/runtime/input",draft:!1,editUrl:"https://github.com/vrchat-community/ClientSim/edit/main/Docs/Source/systems/runtime/input.md",tags:[],version:"current",frontMatter:{id:"input",title:"Input",hide_title:!0},sidebar:"tutorialSidebar",previous:{title:"HighlightManager",permalink:"/systems/runtime/highlight-manager"},next:{title:"InteractiveLayerProvider",permalink:"/systems/runtime/interactive-layer-provider"}},p={},u=[{value:"Input Events",id:"input-events",level:2},{value:"Input Bindings",id:"input-bindings",level:2},{value:"UdonInput",id:"udoninput",level:2}],l={toc:u},c="wrapper";function d(e){let{components:t,...n}=e;return(0,r.kt)(c,(0,i.Z)({},l,n,{components:t,mdxType:"MDXLayout"}),(0,r.kt)("h1",{id:"input"},"Input"),(0,r.kt)("p",null,"In ClientSim, all input calls are in one class to handle input and send events. The ClientSimInputManager uses the new Input System, allowing for event-driven input. It uses the PlayerInput component to gain access to the specific input events based on the Input Bindings displayed below. Since the new Unity Input System package is not included by default, and Unity requires a special setting to enable, all references to the Input System are wrapped in define conditions, which prevents errors when importing into new projects."),(0,r.kt)("h2",{id:"input-events"},"Input Events"),(0,r.kt)("p",null,"Similar to the ",(0,r.kt)("a",{parentName:"p",href:"/systems/runtime/event-dispatcher"},"EventDispatcher"),", the InputManager also has its own Events that different systems can listen to directly. These events are separated from the EventDispatcher itself because all input events have similar parameters and also has input values that are not broadcasted through events but require the listening system to poll for updated axis values."),(0,r.kt)("h2",{id:"input-bindings"},"Input Bindings"),(0,r.kt)("p",null,"The Input System also allows for different bindings for various control schemes. See below for the included bindings: KeyboardMouse, Gamepad, and Experimental XR Controller bindings. Note that XR input bindings within the InputSystem are very limited in Unity 2019. The InputManager will need to be expanded to properly support various VR Controllers"),(0,r.kt)("h2",{id:"udoninput"},"UdonInput"),(0,r.kt)("p",null,"The UdonInput system is part of the InputManager Prefab, which subscribes to the proper events in the InputManager and also polls for updates on movement and look-based inputs. Due to the timing of when Unity sends input events, and when Udon should receive input events, all button-based input is queued and processed later in the frame at the same time as movement and look-based input. This queuing and processing allows input events to happen after Udon\u2019s update method is called, similar to how it is in VRChat."))}d.isMDXComponent=!0}}]);