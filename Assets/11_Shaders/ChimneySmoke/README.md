# Stylized chimney smoke

Drag `ChimneySmoke.prefab` into a scene and place its transform at the chimney opening. The particle plume rises along local +Y, so rotate the prefab if the chimney points in another direction. Parent it to the chimney if it should follow the building.

The prefab includes a looping, prewarmed Particle System, a custom puff texture and a transparent URP shader. Tune the particle system's emission rate, lifetime, start size and noise to change the plume. Change the `_Tint` color in `Materials/ChimneySmoke.mat` to recolor the smoke.
