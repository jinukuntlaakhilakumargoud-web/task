
from fastapi import FastAPI
from pydantic import BaseModel
import tensorflow as tf
import numpy as np
from scipy.signal import butter, lfilter
import os

# --- Constants and Configuration ---
MODEL_PATH = "model/ecg_arrhythmia_model.h5"
CLASS_NAMES = {
    0: 'Normal',
    1: 'Supraventricular Ectopic',
    2: 'Ventricular Ectopic',
    3: 'Fusion',
    4: 'Unknown'
}
FS = 125  # Sampling frequency of the dataset
LOWCUT = 0.5
HIGHCUT = 45.0

# --- FastAPI App Initialization ---
app = FastAPI(title="ECG Arrhythmia Diagnosis API")
model = None

# --- Pydantic Models for Request and Response ---
class EcgRequest(BaseModel):
    signal: list[float]

class DiagnosisResponse(BaseModel):
    arrhythmia_type: str
    confidence: float
    class_id: int

# --- Preprocessing Functions ---
def butter_lowpass_filter(data, cutoff, fs, order=5):
    """Applies a Butterworth low-pass filter to the data."""
    nyq = 0.5 * fs
    normal_cutoff = cutoff / nyq
    b, a = butter(order, normal_cutoff, btype='low', analog=False)
    y = lfilter(b, a, data)
    return y

def preprocess_input(signal: np.ndarray) -> np.ndarray:
    """
    Preprocesses a single raw ECG signal for model prediction.
    - Applies a low-pass filter.
    - Normalizes the signal.
    - Reshapes for the model input.
    """
    # 1. Filtering
    filtered_signal = butter_lowpass_filter(signal, HIGHCUT, FS, order=5)

    # 2. Normalization (Min-Max)
    normalized_signal = (filtered_signal - np.min(filtered_signal)) / (np.max(filtered_signal) - np.min(filtered_signal))

    # 3. Reshaping for model input
    # The model expects a shape of (batch_size, steps, features) -> (1, 187, 1)
    reshaped_signal = normalized_signal.reshape(1, -1, 1)

    # Ensure the signal length is 187 (pad or truncate if necessary)
    # This step is crucial if the input signal isn't exactly 187 samples.
    if reshaped_signal.shape[1] > 187:
        reshaped_signal = reshaped_signal[:, :187, :]
    elif reshaped_signal.shape[1] < 187:
        padding = np.zeros((1, 187 - reshaped_signal.shape[1], 1))
        reshaped_signal = np.concatenate([reshaped_signal, padding], axis=1)

    return reshaped_signal

# --- Model Loading ---
@app.on_event("startup")
def load_model():
    """Load the Keras model on application startup."""
    global model
    if os.path.exists(MODEL_PATH):
        print(f"Loading model from: {MODEL_PATH}")
        model = tf.keras.models.load_model(MODEL_PATH)
        print("Model loaded successfully.")
    else:
        print(f"ERROR: Model file not found at {MODEL_PATH}")
        # In a real application, you might want to prevent the app from starting.
        # For this example, we'll allow it to run but predictions will fail.
        model = None

# --- API Endpoints ---
@app.get("/", summary="Health Check")
def read_root():
    """Root endpoint for health check."""
    return {"status": "ok", "model_loaded": model is not None}

@app.post("/predict", response_model=DiagnosisResponse, summary="Diagnose ECG Signal")
def predict(request: EcgRequest):
    """
    Accepts a raw ECG signal, preprocesses it, and returns the arrhythmia diagnosis.
    """
    if model is None:
        return {"error": "Model is not loaded. Cannot perform prediction."}

    # Convert the input list to a NumPy array
    input_signal = np.array(request.signal, dtype=np.float32)

    # Preprocess the input
    processed_signal = preprocess_input(input_signal)

    # Get model prediction
    prediction = model.predict(processed_signal)

    # Get the class with the highest probability
    predicted_class_id = np.argmax(prediction, axis=1)[0]
    confidence = float(np.max(prediction))

    # Get the corresponding class name
    arrhythmia_type = CLASS_NAMES.get(predicted_class_id, "Unknown")

    return DiagnosisResponse(
        arrhythmia_type=arrhythmia_type,
        confidence=confidence,
        class_id=int(predicted_class_id)
    )

if __name__ == "__main__":
    import uvicorn
    # This allows running the app directly for testing.
    # The model path assumes you run this from the `backend` directory.
    # Example: python main.py
    # Note: The model path needs to be adjusted if run from the repo root.
    # We will assume a production server like gunicorn would run this.
    uvicorn.run(app, host="0.0.0.0", port=8000)
