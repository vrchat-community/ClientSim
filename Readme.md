# ClientSim

![ClientSim in Unity Editor](Tools/Docusaurus/static/images/editor-screenshot.png)

The VRChat Client Simulator, or ClientSim for short, is a tool that enables you to test your VRChat SDK3 Worlds directly in Unity! You can look at the state of all objects to verify things directly.

This repository contains a copy of the source code from the latest release to enable contributons from our community.

## Contributing

Thanks for considering contributing to the ClientSim project!
We will review all pull requests and merge those that improve the project, or leave constructive feedback for contributions that needs more work or changes.

## Repo Changes and Known Issues

In 2025, we updated this repo to enable easier bi-directional sync between the source code in our private repo and this public one.
This change includes:
* Moving the docs from this repo to the main Creator Docs site, which accepts contributions through [the creator-docs repo](https://github.com/vrchat-community/creator-docs).
* Changing the repo from being a standalone Unity project with ClientSim as a package to simply storing the ClientSim source in the `Source` folder. The Unity Project-and-Embedded Package approach no longer works since we merged ClientSim into the VRChat SDK, so we've removed all the unusable bits.
* Removing the Tests folder from the source. This is unfortunate, but they're not currently working - we've got tasks to fix them up but we've got much higher-priority items in delivering and supporting new features, so this reflects the current state of the package.

We've retained the previous state of the repo in the [legacy](/tree/legacy) branch so all the olds docs, tools and tests are available for review there.

## Copyright

Copyright (c) 2025 VRChat
See License.md for full usage information

## Credits

Based on [CyanEmu](https://github.com/CyanLaser/CyanEmu) by [CyanLaser](https://github.com/CyanLaser), who also made this version.
