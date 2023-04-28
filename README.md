<!DOCTYPE html>
<html>
<head>
</head>
<body>
	<h1>Bounding Volume Hierarchy System for Unity Gameobjects</h1>
	![Alt Text](https://media.giphy.com/media/v1.Y2lkPTc5MGI3NjExMDVjZDllZTM4NzhkODQ3ODIzNDU3ZWU4Y2M4YmI1ZDI5YmIwYmQ5NiZlcD12MV9pbnRlcm5hbF9naWZzX2dpZklkJmN0PWc/GrCosevhizMaNytSFh/giphy-downsized-large.gif)
	<p>This GitHub repository contains a Bounding Volume Hierarchy (BVH) system for Unity gameobjects. The system is designed to improve the performance of raycasting in Unity by organizing gameobjects into a binary tree structure using AABB (Axis-Aligned Bounding Boxes) for leaf nodes.</p>
	<h2>Features</h2>
	<ul>
		<li>Supports both 2D and 3D Unity gameobjects.</li>
		<li>Uses a dynamic, incremental approach to build BVH hierarchies in a binary tree.</li>
		<li>Significantly improves raycasting performance by bypassing the expensive Unity EventSystem.RaycastAll call.</li>
		<li>Encapsulates gameobjects into binary tree leaves using AABB.</li>
		<li>Uses Surface Area Heuristic (SAH) to create hierarchies, which is particularly useful when there are a large number of Raycasters in a scene.</li>
	</ul>
	<h2>How to Use</h2>
	<p>To use this system, simply add the DBVHCanvas.cs script to your canvas gameobject. This script manages the BVH hierarchy and is responsible for building, updating, and querying the binary tree structure. You can then call the BinaryTree.Raycast() method to perform fast and efficient raycasts in your game.</p>
	<h3>Example Usage</h3>
<h2>Contributing</h2>
<p>Contributions to this repository are welcome and encouraged! If you find a bug or have an idea for an improvement, please feel free to open an issue or submit a pull request.</p>
<h2>License</h2>
<p>This BVH system is licensed under the MIT License. See the LICENSE file for more information.</p>

</body>
</html>
