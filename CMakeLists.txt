cmake_minimum_required(VERSION 3.20)

project(BrokenArrowTextureCreator LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

set(CMAKE_AUTOMOC ON)
set(CMAKE_AUTOUIC ON)
set(CMAKE_AUTORCC ON)

find_package(Qt6 REQUIRED COMPONENTS Widgets)

add_executable(broken_arrow_texture_creator
    src/main.cpp
)

target_link_libraries(broken_arrow_texture_creator
    PRIVATE Qt6::Widgets
)
