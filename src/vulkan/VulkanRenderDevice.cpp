#include "vulkan/VulkanRenderDevice.h"

#include <stdexcept>

#include "vulkan/VulkanCommandContext.h"

namespace renderer::vulkan_backend {

namespace {
VmaAllocatorCreateInfo BuildAllocatorCreateInfo(const vk::PhysicalDevice& physical_device,
                                               const vk::Device& device, const vk::Instance& instance) {
  VmaAllocatorCreateInfo info{};
  info.physicalDevice = physical_device;
  info.device = device;
  info.instance = instance;
  return info;
}

vk::Format ToVkFormat(Format format) {
  switch (format) {
    case Format::kR8G8B8A8Unorm:
      return vk::Format::eR8G8B8A8Unorm;
    case Format::kR16G16B16A16Float:
      return vk::Format::eR16G16B16A16Sfloat;
    case Format::kR32Float:
      return vk::Format::eR32Sfloat;
  }
  return vk::Format::eUndefined;
}
}

VulkanRenderDevice::VulkanRenderDevice() {
  createInstance();
  createDevice();
  createAllocator();
}

VulkanRenderDevice::~VulkanRenderDevice() {
  if (allocator_) {
    vmaDestroyAllocator(allocator_);
    allocator_ = nullptr;
  }
}

DeviceInfo VulkanRenderDevice::info() const {
  vk::PhysicalDeviceProperties properties = physical_device_.getProperties();
  return DeviceInfo{properties.deviceName, properties.apiVersion};
}

CommandContextPtr VulkanRenderDevice::createCommandContext(std::string name) {
  return std::make_unique<VulkanCommandContext>(*this, std::move(name));
}

std::unique_ptr<Texture2D> VulkanRenderDevice::createTexture2D(Extent2D extent, Format format,
                                                               std::string name) {
  auto texture = std::make_unique<Texture2D>(extent, format, std::move(name));
  (void)ToVkFormat(format);
  return texture;
}

std::unique_ptr<Buffer> VulkanRenderDevice::createBuffer(size_t size_bytes, std::string name) {
  return std::make_unique<Buffer>(size_bytes, std::move(name));
}

std::unique_ptr<ComputePipeline> VulkanRenderDevice::createComputePipeline(
    std::string name, const std::vector<uint32_t>& spirv_bytecode) {
  (void)spirv_bytecode;
  return std::make_unique<ComputePipeline>(std::move(name));
}

void VulkanRenderDevice::submit(CommandContext& context) {
  auto* vk_context = dynamic_cast<VulkanCommandContext*>(&context);
  if (!vk_context) {
    throw std::runtime_error("VulkanRenderDevice submit called with non-Vulkan context");
  }

  vk::SubmitInfo submit_info{};
  submit_info.commandBufferCount = 1;
  vk::CommandBuffer command_buffer = vk_context->command_buffer();
  submit_info.pCommandBuffers = &command_buffer;
  compute_queue_.submit(submit_info, vk::Fence());
}

void VulkanRenderDevice::waitIdle() {
  device_->waitIdle();
}

void VulkanRenderDevice::createInstance() {
  vk::ApplicationInfo app_info{};
  app_info.pApplicationName = "Broken Arrow Texture Creator";
  app_info.apiVersion = VK_API_VERSION_1_3;

  vk::InstanceCreateInfo create_info{};
  create_info.pApplicationInfo = &app_info;

  instance_ = vk::createInstanceUnique(create_info);

  auto physical_devices = instance_->enumeratePhysicalDevices();
  if (physical_devices.empty()) {
    throw std::runtime_error("No Vulkan physical devices found");
  }
  physical_device_ = physical_devices.front();
}

void VulkanRenderDevice::createDevice() {
  auto queue_families = physical_device_.getQueueFamilyProperties();
  for (uint32_t index = 0; index < queue_families.size(); ++index) {
    if (queue_families[index].queueFlags & vk::QueueFlagBits::eCompute) {
      compute_queue_family_index_ = index;
      break;
    }
  }

  float queue_priority = 1.0f;
  vk::DeviceQueueCreateInfo queue_info{};
  queue_info.queueFamilyIndex = compute_queue_family_index_;
  queue_info.queueCount = 1;
  queue_info.pQueuePriorities = &queue_priority;

  vk::DeviceCreateInfo device_info{};
  device_info.queueCreateInfoCount = 1;
  device_info.pQueueCreateInfos = &queue_info;

  device_ = physical_device_.createDeviceUnique(device_info);
  compute_queue_ = device_->getQueue(compute_queue_family_index_, 0);
}

void VulkanRenderDevice::createAllocator() {
  VmaAllocatorCreateInfo allocator_info = BuildAllocatorCreateInfo(physical_device_, device_.get(),
                                                                   instance_.get());
  if (vmaCreateAllocator(&allocator_info, &allocator_) != VK_SUCCESS) {
    throw std::runtime_error("Failed to create VMA allocator");
  }
}

}  // namespace renderer::vulkan_backend
