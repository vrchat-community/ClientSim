"use strict";(self.webpackChunkclient_sim=self.webpackChunkclient_sim||[]).push([[9677],{3905:(e,t,n)=>{n.d(t,{Zo:()=>y,kt:()=>m});var r=n(7294);function a(e,t,n){return t in e?Object.defineProperty(e,t,{value:n,enumerable:!0,configurable:!0,writable:!0}):e[t]=n,e}function o(e,t){var n=Object.keys(e);if(Object.getOwnPropertySymbols){var r=Object.getOwnPropertySymbols(e);t&&(r=r.filter((function(t){return Object.getOwnPropertyDescriptor(e,t).enumerable}))),n.push.apply(n,r)}return n}function s(e){for(var t=1;t<arguments.length;t++){var n=null!=arguments[t]?arguments[t]:{};t%2?o(Object(n),!0).forEach((function(t){a(e,t,n[t])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(n)):o(Object(n)).forEach((function(t){Object.defineProperty(e,t,Object.getOwnPropertyDescriptor(n,t))}))}return e}function i(e,t){if(null==e)return{};var n,r,a=function(e,t){if(null==e)return{};var n,r,a={},o=Object.keys(e);for(r=0;r<o.length;r++)n=o[r],t.indexOf(n)>=0||(a[n]=e[n]);return a}(e,t);if(Object.getOwnPropertySymbols){var o=Object.getOwnPropertySymbols(e);for(r=0;r<o.length;r++)n=o[r],t.indexOf(n)>=0||Object.prototype.propertyIsEnumerable.call(e,n)&&(a[n]=e[n])}return a}var c=r.createContext({}),l=function(e){var t=r.useContext(c),n=t;return e&&(n="function"==typeof e?e(t):s(s({},t),e)),n},y=function(e){var t=l(e.components);return r.createElement(c.Provider,{value:t},e.children)},p="mdxType",h={inlineCode:"code",wrapper:function(e){var t=e.children;return r.createElement(r.Fragment,{},t)}},d=r.forwardRef((function(e,t){var n=e.components,a=e.mdxType,o=e.originalType,c=e.parentName,y=i(e,["components","mdxType","originalType","parentName"]),p=l(n),d=a,m=p["".concat(c,".").concat(d)]||p[d]||h[d]||o;return n?r.createElement(m,s(s({ref:t},y),{},{components:n})):r.createElement(m,s({ref:t},y))}));function m(e,t){var n=arguments,a=t&&t.mdxType;if("string"==typeof e||a){var o=n.length,s=new Array(o);s[0]=d;var i={};for(var c in t)hasOwnProperty.call(t,c)&&(i[c]=t[c]);i.originalType=e,i[p]="string"==typeof e?e:a,s[1]=i;for(var l=2;l<o;l++)s[l]=n[l];return r.createElement.apply(null,s)}return r.createElement.apply(null,n)}d.displayName="MDXCreateElement"},9726:(e,t,n)=>{n.r(t),n.d(t,{assets:()=>c,contentTitle:()=>s,default:()=>h,frontMatter:()=>o,metadata:()=>i,toc:()=>l});var r=n(7462),a=(n(7294),n(3905));const o={id:"synced-object-manager",title:"SyncedObjectManager",hide_title:!0},s="SyncedObjectManager",i={unversionedId:"systems/runtime/synced-object-manager",id:"systems/runtime/synced-object-manager",title:"SyncedObjectManager",description:"The SyncedObjectManager keeps track of all initialized synced objects (IClientSimSyncable) in the scene. These synced objects are put into two lists: one list for all synced objects, and another for all position-synced objects. The SyncedObjectManager currently has only two main functions. The first is to check all position-synced objects to verify they are above the respawn height. If they fall below the respawn height, they are respawned to their start position or destroyed, depending on the settings in the SceneManager. The second function is to ensure objects have the correct owners when a player leaves. The manager listens for the OnPlayerLeft Event, goes through all objects to check if that player was the  owner, and then sets those objects to be owned by the master player instead. This ownership transfer happens before Udon Programs are notified of the player leaving.",source:"@site/docs/systems/runtime/synced-object-manager.md",sourceDirName:"systems/runtime",slug:"/systems/runtime/synced-object-manager",permalink:"/systems/runtime/synced-object-manager",draft:!1,editUrl:"https://github.com/vrchat-community/ClientSim/edit/main/Docs/Source/systems/runtime/synced-object-manager.md",tags:[],version:"current",frontMatter:{id:"synced-object-manager",title:"SyncedObjectManager",hide_title:!0},sidebar:"tutorialSidebar",previous:{title:"Settings",permalink:"/systems/runtime/settings"},next:{title:"TooltipManager",permalink:"/systems/runtime/tooltip-manager"}},c={},l=[],y={toc:l},p="wrapper";function h(e){let{components:t,...n}=e;return(0,a.kt)(p,(0,r.Z)({},y,n,{components:t,mdxType:"MDXLayout"}),(0,a.kt)("h1",{id:"syncedobjectmanager"},"SyncedObjectManager"),(0,a.kt)("p",null,"The SyncedObjectManager keeps track of all initialized synced objects (IClientSimSyncable) in the scene. These synced objects are put into two lists: one list for all synced objects, and another for all position-synced objects. The SyncedObjectManager currently has only two main functions. The first is to check all position-synced objects to verify they are above the respawn height. If they fall below the respawn height, they are respawned to their start position or destroyed, depending on the settings in the ",(0,a.kt)("a",{parentName:"p",href:"/systems/runtime/scene-manager"},"SceneManager"),". The second function is to ensure objects have the correct owners when a player leaves. The manager listens for the OnPlayerLeft ",(0,a.kt)("a",{parentName:"p",href:"/systems/runtime/event-dispatcher"},"Event"),", goes through all objects to check if that player was the  owner, and then sets those objects to be owned by the master player instead. This ownership transfer happens before Udon Programs are notified of the player leaving."),(0,a.kt)("p",null,"VRC.SDK3.ClientSim.ClientSimSyncedObjectManager"))}h.isMDXComponent=!0}}]);