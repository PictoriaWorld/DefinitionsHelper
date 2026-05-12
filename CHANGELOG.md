# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/).

## [1.0.4] - 12-05-2026

### Fixed
- `.pstr` artwork receives a small canvas-display compensation so thumbnails visually align more closely with `_preview.png` thumbnails.

## [1.0.3] - 12-05-2026

### Fixed
- `.pstr` thumbnails keep the Pictoria badge anchored to the thumbnail canvas instead of covering very small preview images.

## [1.0.2] - 12-05-2026

### Fixed
- Small `.pstr` thumbnails no longer upscale beyond the size of the embedded preview image.

## [1.0.1] - 12-05-2026

### Fixed
- `.pstr` thumbnails now match the sizing behavior of their sibling `_preview.png` thumbnails in Windows Explorer.

## [1.0.0] - 25-03-2026

### Added
- `.pstr` files display the structure's first frame as their thumbnail in Windows Explorer, with a Pictoria logo overlay. If the structure has no frames, just the Pictoria logo is displayed.
- `.pstr` and `.ppty` files display the Pictoria logo as their file type icon (displayed when using Windows Explorer details/list view modes).
