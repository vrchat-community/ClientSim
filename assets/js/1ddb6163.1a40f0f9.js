"use strict";(self.webpackChunkclient_sim=self.webpackChunkclient_sim||[]).push([[113],{3905:function(e,t,n){n.d(t,{Zo:function(){return c},kt:function(){return p}});var r=n(7294);function i(e,t,n){return t in e?Object.defineProperty(e,t,{value:n,enumerable:!0,configurable:!0,writable:!0}):e[t]=n,e}function o(e,t){var n=Object.keys(e);if(Object.getOwnPropertySymbols){var r=Object.getOwnPropertySymbols(e);t&&(r=r.filter((function(t){return Object.getOwnPropertyDescriptor(e,t).enumerable}))),n.push.apply(n,r)}return n}function a(e){for(var t=1;t<arguments.length;t++){var n=null!=arguments[t]?arguments[t]:{};t%2?o(Object(n),!0).forEach((function(t){i(e,t,n[t])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(n)):o(Object(n)).forEach((function(t){Object.defineProperty(e,t,Object.getOwnPropertyDescriptor(n,t))}))}return e}function s(e,t){if(null==e)return{};var n,r,i=function(e,t){if(null==e)return{};var n,r,i={},o=Object.keys(e);for(r=0;r<o.length;r++)n=o[r],t.indexOf(n)>=0||(i[n]=e[n]);return i}(e,t);if(Object.getOwnPropertySymbols){var o=Object.getOwnPropertySymbols(e);for(r=0;r<o.length;r++)n=o[r],t.indexOf(n)>=0||Object.prototype.propertyIsEnumerable.call(e,n)&&(i[n]=e[n])}return i}var l=r.createContext({}),u=function(e){var t=r.useContext(l),n=t;return e&&(n="function"==typeof e?e(t):a(a({},t),e)),n},c=function(e){var t=u(e.components);return r.createElement(l.Provider,{value:t},e.children)},m={inlineCode:"code",wrapper:function(e){var t=e.children;return r.createElement(r.Fragment,{},t)}},d=r.forwardRef((function(e,t){var n=e.components,i=e.mdxType,o=e.originalType,l=e.parentName,c=s(e,["components","mdxType","originalType","parentName"]),d=u(n),p=i,f=d["".concat(l,".").concat(p)]||d[p]||m[p]||o;return n?r.createElement(f,a(a({ref:t},c),{},{components:n})):r.createElement(f,a({ref:t},c))}));function p(e,t){var n=arguments,i=t&&t.mdxType;if("string"==typeof e||i){var o=n.length,a=new Array(o);a[0]=d;var s={};for(var l in t)hasOwnProperty.call(t,l)&&(s[l]=t[l]);s.originalType=e,s.mdxType="string"==typeof e?e:i,a[1]=s;for(var u=2;u<o;u++)a[u]=n[u];return r.createElement.apply(null,a)}return r.createElement.apply(null,n)}d.displayName="MDXCreateElement"},3165:function(e,t,n){n.r(t),n.d(t,{assets:function(){return c},contentTitle:function(){return l},default:function(){return p},frontMatter:function(){return s},metadata:function(){return u},toc:function(){return m}});var r=n(7462),i=n(3366),o=(n(7294),n(3905)),a=["components"],s={id:"udon-manager",title:"UdonManager",hide_title:!0},l="UdonManager",u={unversionedId:"systems/runtime/udon-manager",id:"systems/runtime/udon-manager",title:"UdonManager",description:"The UdonManager keeps track of all initialized UdonBehaviours in the scene. Note that with the VRCSDK, an UdonBehaviour will not initialize if it does not have a program. This means that legacy position-synced UdonBehaviours without programs are not tracked, even with the SyncedObjectManager. The UdonManager has two main roles. The first is to notify all Udon Helpers when ClientSim has finished initializing, which allows UdonBehaviours to start. The second is to listen for certain ClientSim Events to forward to all UdonBehaviours. Currently the UdonManager only forwards the following events:",source:"@site/docs/systems/runtime/udon-manager.md",sourceDirName:"systems/runtime",slug:"/systems/runtime/udon-manager",permalink:"/ClientSim/systems/runtime/udon-manager",tags:[],version:"current",frontMatter:{id:"udon-manager",title:"UdonManager",hide_title:!0},sidebar:"tutorialSidebar",previous:{title:"TooltipManager",permalink:"/ClientSim/systems/runtime/tooltip-manager"},next:{title:"Unity Event System",permalink:"/ClientSim/systems/runtime/unity-event-system"}},c={},m=[],d={toc:m};function p(e){var t=e.components,n=(0,i.Z)(e,a);return(0,o.kt)("wrapper",(0,r.Z)({},d,n,{components:t,mdxType:"MDXLayout"}),(0,o.kt)("h1",{id:"udonmanager"},"UdonManager"),(0,o.kt)("p",null,"The UdonManager keeps track of all initialized UdonBehaviours in the scene. Note that with the VRCSDK, an UdonBehaviour will not initialize if it does not have a program. This means that legacy position-synced UdonBehaviours without programs are not tracked, even with the SyncedObjectManager. The UdonManager has two main roles. The first is to notify all Udon Helpers when ClientSim has finished initializing, which allows UdonBehaviours to start. The second is to listen for certain ClientSim ",(0,o.kt)("a",{parentName:"p",href:"/ClientSim/systems/runtime/event-dispatcher"},"Events")," to forward to all UdonBehaviours. Currently the UdonManager only forwards the following events:"),(0,o.kt)("ul",null,(0,o.kt)("li",{parentName:"ul"},"OnPlayerJoined"),(0,o.kt)("li",{parentName:"ul"},"OnPlayerLeft"),(0,o.kt)("li",{parentName:"ul"},"OnPlayerRespawn")))}p.isMDXComponent=!0}}]);