# Graphics Backend Requirements & Strategy

## GPU Requirements

### Formats
- `R8G8B8A8_UNORM` for standard texture I/O and editor previews.
- `R16G16B16A16_FLOAT` for HDR intermediates and compute outputs.
- `R32_FLOAT` for single-channel compute and metadata buffers.

### Compression
- Prefer BC7 (desktop) or ASTC (mobile) targets for final export.
- Raw upload path must accept uncompressed RGBA8.
- Compression is handled offline/on-demand, not during the minimal compute prototype.

### Compute Shaders
- Must support Vulkan 1.3 compute pipeline dispatch.
- Workgroup size target: 8x8x1 for texture kernels.
- Pipeline should allow descriptor bindings for input/output textures and uniform buffers.

### Async Transfer
- Dedicated transfer queue preferred; fallback to compute queue if unavailable.
- Staging buffers used for upload/readback with explicit queue ownership transfers.
- Synchronization via timeline semaphores or fences, with per-frame staging pools.

## Backend Extension Strategy

### Interface Layer
- Keep `RenderDevice`, `CommandContext`, `Resource`, `Pipeline` as the cross-API façade.
- Implement per-backend subclasses in `src/vulkan`, `src/dx12`, `src/metal` as needed.

### Feature Mapping
- Vulkan: Vulkan-Hpp + VMA for device ownership and memory allocation.
- DX12: use D3D12MA for allocator parity; keep resource state transitions explicit.
- Metal: translate resource lifecycle into `MTLResource` and `MTLCommandBuffer` patterns.

### Shader Pipeline
- Standardize shader inputs via SPIR-V + cross-compilation (SPIRV-Cross or DXC) for
  DXIL/MetalSL outputs.
- Maintain a shared reflection format to bind resources uniformly.

### Migration Plan
- Keep abstraction narrow and focused on compute + texture workflows.
- Expand resource descriptors and pipeline layouts only when DX12/Metal parity requires it.
- Add backend-specific capability queries to gate features like async transfer queues.
