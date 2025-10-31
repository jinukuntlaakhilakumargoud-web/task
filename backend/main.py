
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

class ChatRequest(BaseModel):
    message: str

class ChatResponse(BaseModel):
    reply: str

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

# --- Chatbot Logic ---
CHATBOT_RESPONSES = {
    "normal": "A 'Normal' heartbeat, or Normal Sinus Rhythm, means the heart's electrical impulse originates from the sinus node and follows a normal pathway, resulting in a regular rhythm and rate (usually 60-100 bpm).",
    "supraventricular ectopic": "'Supraventricular Ectopic' (SVE) beat is a premature heartbeat originating above the ventricles, often in the atria. It can feel like a skipped beat or flutter and is usually benign.",
    "ventricular ectopic": "'Ventricular Ectopic' (VE) beat is a premature heartbeat originating from within the ventricles. It feels like a skipped or forceful beat. Occasional VEs are common, but frequent ones may indicate underlying issues.",
    "fusion": "A 'Fusion' beat occurs when a supraventricular and a ventricular impulse coincide to produce a hybrid beat. It's often seen in the context of ventricular arrhythmias.",
    "unknown": "An 'Unknown' beat is a heartbeat that could not be confidently classified into one of the other categories by the model. This may require further analysis by a medical professional.",
    "hello": "Hello! I am an arrhythmia information bot. Ask me about a type of heartbeat (e.g., 'What is a Normal beat?') to learn more.",
    "default": "I can provide information on the following arrhythmia types: Normal, Supraventricular Ectopic, Ventricular Ectopic, and Fusion. Please ask me about one of them."
}

def get_chatbot_response(message: str) -> str:
    """Simple rule-based logic for the chatbot."""
    message_lower = message.lower()
    if "normal" in message_lower:
        return CHATBOT_RESPONSES["normal"]
    if "supraventricular" in message_lower:
        return CHATBOT_RESPONSES["supraventricular ectopic"]
    if "ventricular" in message_lower:
        return CHATBOT_RESPONSES["ventricular ectopic"]
    if "fusion" in message_lower:
        return CHATBOT_RESPONSES["fusion"]
    if "unknown" in message_lower:
        return CHATBOT_RESPONSES["unknown"]
    if "hello" in message_lower or "hi" in message_lower:
        return CHATBOT_RESPONSES["hello"]
    return CHATBOT_RESPONSES["default"]

@app.post("/chat", response_model=ChatResponse, summary="Get Chatbot Response")
def chat(request: ChatRequest):
    """
    Accepts a user message and returns a predefined chatbot response.
    """
    reply = get_chatbot_response(request.message)
    return ChatResponse(reply=reply)


if __name__ == "__main__":
    import uvicorn
    # This allows running the app directly for testing.
    # The model path assumes you run this from the `backend` directory.
    # Example: python main.py
    # Note: The model path needs to be adjusted if run from the repo root.
    # We will assume a production server like gunicorn would run this.
    uvicorn.run(app, host="0.0.0.0", port=8000)
