cmake_minimum_required(VERSION 3.20)

project(BrokenArrowTextureCreator LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

add_library(core STATIC
    src/core/core.cpp
)
target_include_directories(core
    PUBLIC
        ${CMAKE_CURRENT_SOURCE_DIR}/include
)

add_library(tools STATIC
    src/tools/tools.cpp
)
target_include_directories(tools
    PUBLIC
        ${CMAKE_CURRENT_SOURCE_DIR}/include
)

add_library(render STATIC
    src/render/render.cpp
)
target_include_directories(render
    PUBLIC
        ${CMAKE_CURRENT_SOURCE_DIR}/include
)

add_library(ui STATIC
    src/ui/ui.cpp
)
target_include_directories(ui
    PUBLIC
        ${CMAKE_CURRENT_SOURCE_DIR}/include
)
target_link_libraries(ui
    PUBLIC
        core
        tools
        render
)
