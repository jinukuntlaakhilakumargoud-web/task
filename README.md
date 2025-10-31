
# Enhancing Arrhythmia Diagnosis through ECG Deep Learning and Augmented Reality

This project provides a comprehensive, multi-platform solution for diagnosing arrhythmia from ECG signals. It includes a machine learning model, a backend API, an Augmented Reality (AR) mobile app, and a user-friendly web interface.

## Project Components

1.  **ML Model (`/ml_model`)**: A 1D Convolutional Neural Network (CNN) built with TensorFlow/Keras, trained on the MIT-BIH Arrhythmia Dataset to classify heartbeats into five categories.
2.  **Backend API (`/backend`)**: A Python FastAPI server that loads the trained model and serves predictions and chatbot responses via a REST API.
3.  **AR App (`/AR_Heart_App`)**: A Unity/C# application that uses AR Foundation to visualize the diagnosis on a 3D heart model in the real world.
4.  **Web Frontend (`/web_frontend`)**: An HTML/CSS/JS single-page application that provides an accessible way to visualize ECG data and interact with the diagnostic tools.

## How to Run This Project

### 1. Train the Machine Learning Model

The model training script downloads the required dataset and trains the 1D CNN. The final model is saved in a format ready for the backend.

```bash
# Navigate to the ml_model directory
cd ml_model

# Install dependencies (if you haven't already)
pip install pandas tensorflow requests

# Run the training script
python3 train_ecg_model.py
```
*Note: This script will save `ecg_arrhythmia_model.h5` inside the `backend/model/` directory.*

### 2. Run the Backend API

The FastAPI server is the core of the application, handling requests from both the AR app and the web frontend.

```bash
# Navigate to the backend directory
cd backend

# Install dependencies
pip install -r requirements.txt

# Start the server
# The server will run on http://127.0.0.1:8000
uvicorn main:app --reload
```

### 3. Run the Web Frontend

The web frontend provides a user-friendly interface in any modern browser. Since it's a simple HTML/JS/CSS application, you can run it by opening the `index.html` file, but it's best served via a simple local web server.

**Using Python's built-in HTTP server (easiest method):**

```bash
# Navigate to the web_frontend directory
cd web_frontend

# Start a simple web server (for Python 3)
python3 -m http.server
```
Now, open your web browser and go to **http://localhost:8000**. You should see the application interface.

### 4. Set Up the AR Application

The AR application requires the Unity Editor. Follow the detailed instructions in the `UNITY_SETUP.md` file to configure the Unity project, connect the scripts, and build the application for your AR-capable mobile device.

---
This project demonstrates a full-stack approach to a machine learning application, from data processing and model training to deployment and multi-platform visualization.
