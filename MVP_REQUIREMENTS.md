# Broken Arrow Texture Creator — MVP Requirements

## 1. Draft User Personas

1. **Indie Game Developers**
   - Small teams or solo devs creating stylized or realistic assets.
   - Need a fast, intuitive workflow to produce textures without expensive tools.
2. **Product/Industrial Designers**
   - Need to visualize materials and finishes quickly for product renders.
   - Focused on material libraries, accuracy, and export formats.
3. **3D Hobbyists/Modders**
   - Enthusiasts learning 3D workflows or creating mod assets.
   - Prefer approachable UX, guidance, and presets.
4. **Technical Artists**
   - Integrate asset pipelines and automate texture creation.
   - Need consistent output, batch operations, and format control.
5. **Educators/Students**
   - Learning the fundamentals of UVs, materials, and texturing.
   - Need a learning-friendly UI with clear visual feedback.

## 2. Core Workflows (Prioritized)

1. **Materials & Texturing (Highest Priority)**
   - Import base meshes, apply materials, preview results.
   - Texture painting, procedural materials, and smart materials.
2. **UV Unwrapping & Layout (High Priority)**
   - Auto-UV tools and manual adjustments.
   - UV packing and export to common texture sizes.
3. **Poly Modeling (Medium Priority)**
   - Basic mesh editing (extrude, loop cut, bevel).
   - Sufficient for clean topology for texturing.
4. **Sculpting (Lower Priority for MVP)**
   - Limited sculpting for surface detail.
   - Focus on normal map baking rather than full sculpt toolset.

## 3. Target Platforms & Minimum Hardware Specs

**Target Platforms**
- Windows 10/11 (x64)
- macOS 12+ (Apple Silicon and Intel)
- Linux (Ubuntu 22.04+ or equivalent)

**Minimum Hardware**
- CPU: Quad-core 2.5 GHz
- RAM: 8 GB
- GPU: 2 GB VRAM, OpenGL 4.1 or Vulkan 1.1 support
- Storage: 2 GB available
- Display: 1080p

## 4. MVP Requirements

### Must-Have
- Import/export common mesh formats (OBJ, FBX, glTF).
- Material/texture preview with real-time viewport rendering.
- Basic texture painting (albedo, roughness, metallic).
- UV auto-unwrapping and packing.
- Export texture sets in standard PBR formats.
- Simple project save/load.

### Should-Have
- Procedural material presets (wood, metal, plastic, fabric).
- Bake maps (normal, AO, curvature).
- Layer-based texture workflow with blend modes.
- Basic mesh editing tools (extrude, merge, scale).
- Customizable hotkeys and workspace layouts.

### Nice-to-Have
- Limited sculpting tools with brush presets.
- Smart materials that adapt to mesh curvature and edges.
- Batch export for multiple assets.
- Asset library management and tagging.
- Integrated tutorial/onboarding flow.
