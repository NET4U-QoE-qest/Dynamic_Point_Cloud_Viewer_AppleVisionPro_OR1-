Project Name: QoE_qest_MR_Pointclouds  
Company: NET4U  
Version: 1.8.0  

Description:  
This Unity project provides a Mixed Reality interface tailored for Apple Vision Pro, enabling randomized visualization and subjective evaluation (QoE) of dynamic point clouds in XR.

Main Features:  
- Random playback of dynamic point clouds with randomized quality combinations  
- Point cloud loading from internal storage (Vision Pro filesystem)  
- Animated playback of `.ply` sequences  
- Integrated QoE rating panel (5-level discrete slider + Submit button)  
- Automatic logging of ratings in CSV format  
- Text banner with evaluation progress and current point cloud metadata  

Workflow:  
1. Place your point cloud dataset into the following Unity project path:  
   `../Assets/Project_Net4U_QoE_qest/VisionPro/pc/`    
2.	Follow this folder structure: pc/ ├── GPCC_octree/ ├── GPCC_trisoup/ └── VPCC/
3. Enter the user code in the initial login menu  
4. Evaluate the test point cloud, which is shown first to let the user get familiar with the interface  
5. Start the randomized evaluation sequence:  
   - One point cloud is randomly selected from the dataset  
   - It is displayed at a fixed distance in front of the user  
   - The QoE slider immediately appears for evaluation  
6. The user rates each point cloud using the 10-level slider and taps Submit  
7. At the end of the sequence:  
   - A final banner informs the user that the evaluation session is complete  
   - All ratings are saved locally in:  
`valutazioni_qoe.csv` (stored in `Application.persistentDataPath`)  

File Exclusion (.gitignore):  
- Raw point clouds: `*.ply`, `*.bin`  
- Build and temp folders: `Library/`, `Builds/`, `Temp/`, `*.apk`, `*.xcarchive`  
- User-specific configs and logs  

Build Info:  
- Tested on Apple Vision Pro with Unity + VisionOS SDK  
- Compatible with hand-tracking and passthrough  
- Requires Unity 6+ and Xcode 15+  
- Built using OpenXR backend  

Contact:  
Project maintained by NET4U_QoE_Qest  
GitHub: https://github.com/NET4U-QoE-qest
 
