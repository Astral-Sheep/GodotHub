# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

### Added


### Changed


### Fixed


### Removed


## [0.1.3] - 2024-02-08

### Added
- Tooltip on project path and version path
- Process elevation for engine installation and deletion on windows
- "Update Repo" button to check for new releases without restarting the application
- Icon creation when creating a new project
- Changelog

### Changed
- Tab buttons always have the same space between them

### Fixed
- Focus on "Add" button in installs tab removed
- Install items auto sort when adding a new item
- Check installs config file when checking if an engine is installed
- Date going crazy after new year

## [0.1.2] - 2023-11-05

### Added
- Error popups

### Changed
- Use only alpha for splash screen animation

### Fixed
- Repository first initialization

## [0.1.1] - 2023-09-24

### Added
- Install item instantiation only if the downloaded engine is put in the config file
- Project creation for supported versions

### Changed
- Increased sorting button size in installs tab

### Fixed
- Project items auto sort when adding a new item
- Syntax error in linux and macos builds

## [0.1] - 2023-09-18

### Added
- Releases download from Github
- Projects tab
- Installs tab
- Releases tab
- Settings panel
- Config files
    * Store settings, projects and engine versions
- Release item
    * Release configuration before download
    * OS selection
    * Architecture selection
    * C# support selection
    * Check for installed versions
- Items sorting
- Download status
- Releases storage on disk
- Project item
    * Project opening
    * Project deletion (only in config file)
    * Engine version selection
    * Favorite tag
    * Project item sorting
- Project addition in project tab ("Add" button in project tab)
- Check for project in default folder
- Support for releases greater or equal to 3.1.1
- UI theme
- License
- Readme

[unreleased]: https://github.com/Astral-Sheep/GodotHub/compare/0.1.3...HEAD
[0.1.3]: https://github.com/Astral-Sheep/GodotHub/compare/0.1.2...0.1.3
[0.1.2]: https://github.com/Astral-Sheep/GodotHub/compare/0.1.1...0.1.2
[0.1.1]: https://github.com/Astral-Sheep/GodotHub/compare/0.1...0.1.1
[0.1]: https://github.com/Astral-Sheep/GodotHub/releases/tag/0.1
