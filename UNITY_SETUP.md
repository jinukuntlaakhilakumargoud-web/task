
# Unity Editor Setup Instructions

These instructions guide you through setting up the Unity project, specifically the Animator Controller required for the `HeartAnimator.cs` script to function correctly.

## 1. Find a 3D Heart Model
- Go to a 3D model marketplace like **Sketchfab**, **CGTrader**, or the **Unity Asset Store**.
- Find a suitable 3D model of a human heart. Many are available for free.
- Download the model (usually in `.fbx`, `.obj` format) and import it into your Unity project by dragging the file into your `Assets` folder.

## 2. Create a Prefab
- Drag the imported heart model from your `Assets` into the scene.
- Create a new **Prefab** by dragging the heart GameObject from the Hierarchy window back into your `Assets` folder. You will use this prefab in the `ARController`.
- You can now delete the heart from the scene.

## 3. Create an Animator Controller
- In your `Assets` folder, right-click and go to `Create > Animator Controller`.
- Name this new asset something like `HeartAnimatorController`.
- Attach this controller to your heart prefab by selecting the prefab and dragging the `HeartAnimatorController` into the "Controller" field of the **Animator** component. If the prefab doesn't have an Animator component, add one via `Add Component > Animation > Animator`.

## 4. Create Animation Clips
- You need animations for the different heartbeats. If your model doesn't come with them, you can create them in Unity.
- Select your heart prefab. Open the **Animation** window (`Window > Animation > Animation`).
- Click the "Create" button to create a new animation clip. Name it `NormalBeat`.
- Animate the heart to perform a normal beat (e.g., by scaling the model up and down slightly over a short duration).
- Repeat this process to create clips for `V_EctopicBeat` and `S_EctopicBeat`, making them visually distinct (e.g., a more erratic or faster beat).

## 5. Configure the Animator Controller
- Double-click your `HeartAnimatorController` asset to open the **Animator** window.
- You will see a graph with nodes like `Any State`, `Entry`, and `Exit`.

### a. Create States
- Drag your animation clips (`NormalBeat`, `V_EctopicBeat`, `S_EctopicBeat`) from the `Assets` folder into the Animator window. This will create a new **State** for each animation.
- It's good practice to create an `Idle` state as the default. Right-click in the grid, select `Create State > Empty`, and name it `Idle`.
- Right-click the `Idle` state and choose **"Set as Layer Default State"**. It will turn orange.

### b. Create Triggers (Parameters)
- In the top-left of the Animator window, click the **Parameters** tab.
- Click the `+` icon and select **Trigger**.
- Create three triggers with the exact names used in `HeartAnimator.cs`:
  - `PlayNormal`
  - `PlayV-Ectopic`
  - `PlayS-Ectopic`

### c. Create Transitions
- Transitions define how the Animator moves from one state to another.
- Right-click the `Any State` node and select **"Make Transition"**. Drag the arrow to the `NormalBeat` state.
- Select the newly created transition arrow. In the **Inspector**, under the **Conditions** section, click the `+` button and select `PlayNormal` from the dropdown.
- Repeat this process for the other states:
  - `Any State` -> `V_EctopicBeat` (Condition: `PlayV-Ectopic`)
  - `Any State` -> `S_EctopicBeat` (Condition: `PlayS-Ectopic`)
- You also need transitions to return to the `Idle` state after an animation finishes.
  - Right-click `NormalBeat` and make a transition to `Idle`. Select the transition and ensure **"Has Exit Time"** is checked and set to 1. This means the transition will happen automatically when the animation clip finishes.
  - Do the same for `V_EctopicBeat` -> `Idle` and `S_EctopicBeat` -> `Idle`.

## 6. Final Scene Setup
- Create an empty GameObject in your scene and name it `AR_Session_Origin`. Add the `AR Session Origin` and `AR Plane Manager` components.
- Create another empty GameObject and name it `AppManager`. Attach the `ARController.cs` and `APIManager.cs` scripts to it.
- In the `ARController` component on `AppManager`, drag your `HeartPrefab` from `Assets` into the "Heart Prefab" field.
- Create a UI Button (`GameObject > UI > Button`) and drag it into the "Diagnosis Button" field in the `ARController`.

## 7. Chatbot UI Setup
- Create a new UI Button (`GameObject > UI > Button`) for toggling the chat. Name it `ChatButton`. Drag it into the "Chat Button" field in the `ARController`.
- Create a UI Panel (`GameObject > UI > Panel`) to serve as the chat window. Name it `ChatPanel`. Drag this into the "Chat Panel" field in the `ARController`.
- Attach the `ChatManager.cs` script to the `ChatPanel` GameObject.
- Inside the `ChatPanel`, create the following UI elements:
  - A **Scroll View** (`GameObject > UI > Scroll View`) for the chat history.
  - An **Input Field** (`GameObject > UI > Input Field`) for user messages.
  - A **Button** (`GameObject > UI > Button`) for sending messages.
- Configure the `ChatManager` component on the `ChatPanel`:
  - Drag the **Input Field** to the `Chat Input Field`.
  - Drag the **Send Button** to the `Send Button`.
  - Drag the **Scroll View** to the `Chat Scroll Rect`.
  - Drag the `AppManager` GameObject (which has the `APIManager` script) to the `Api Manager` field.
  - The `Chat History Text` needs a `Text` component. You can find one as a child of the `Scroll View`'s `Content` object. Drag this `Text` object into the `Chat History Text` field.

Your Unity project is now set up! When you run the app on an AR-capable device, you can tap to place the heart, and then press the buttons to trigger the diagnosis and animation, or to open the chat window.
