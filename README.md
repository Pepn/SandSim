# Sand Simulation in Unity

## Introduction
This project is a simple GPU sand simulation implemented in Unity. The simulation utilizes cellular automaton principles to model the behavior of sand, water, and stone.

## Usage
To interact with the sand simulation, use the following controls:
- <b>Left Click</b>: Spawn sand particles at the cursor's position.
- <b>Right Click</b>: Delete pixels at the cursor's position.
- <b>Scroll Click</b>: Spawn stone particles at the cursor's position.
- <b>Side Button Click</b>: Spawn water particles at the cursor's position.

Ensure that the product of <code>numThreads</code> and <code>numGroups</code> equals the <code>textureSize</code> so the complete texture is used.

## Features
- <b>Sand, Water, and Stone Simulation</b>: The simulation supports three different types of particles: sand, water, and stone. Each particle type exhibits distinct behavior.
- <b>Pixel Spawning</b>: The compute shader dynamically spawns particles using the mouse.

## Implementation Details
- <b>Compute Shader</b>: The simulation is completely on the GPU side, allowing for parallel processing and efficient simulation of large particle systems.
- <b>Particle Interaction</b>: Particle interaction is governed by a set of rules defined within the compute shader, ensuring realistic particle behavior.

## Future Improvements
- <b>Optimization</b>: More efficient CPU -> GPU spawn pixel buffer, currently we send a complete buffer (lol).
- <b>Additional Particle Types</b>: The simulation can be expanded to include additional particle types, such as plants or animals, to create more diverse and dynamic environments.
- <b>Enhanced Visuals</b>: Implementing dynamic lighting and shader effects can enhance the visual appeal of the simulation, creating a more immersive experience.
- <b>Enhanced Visuals</b>: Improve the rules for the water cellular automata, they are currently quite scuffed.
