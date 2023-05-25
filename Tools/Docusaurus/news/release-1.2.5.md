---
slug: release-1.2.5
date: 2023-05-25
title: Release 1.2.5
authors: [cubed]
tags: [release]
draft: false
---
## Summary

Fixed a handful of ClientSim issues

### Changes
- Fixed Destroying focused object breaks ClientSim [Issue 59](https://github.com/vrchat-community/ClientSim/issues/59)
- Fixed LineRenderer.BakeMesh() throws errors [Issue 49](https://github.com/vrchat-community/ClientSim/issues/49)
- Fixed Sequential Spawn Order is broken [Issue 36](https://github.com/vrchat-community/ClientSim/issues/36)
- Fixed Error about missing scene descriptor erroneously says "world descriptor" [Issue 66](https://github.com/vrchat-community/ClientSim/issues/66)
- Removed simulation of bug that prevents movement when exiting stations (bugfix will be in the next client release) [PR 73](https://github.com/vrchat-community/ClientSim/pull/73)
