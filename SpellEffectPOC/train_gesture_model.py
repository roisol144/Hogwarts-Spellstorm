import numpy as np
import tensorflow as tf
from tensorflow import keras
import os
import json
import requests
import gzip
import shutil

# Download Quick, Draw! data for circle, square, and Z
def download_quickdraw_data():
    categories = ['circle', 'square', 'zigzag']
    base_url = 'https://storage.googleapis.com/quickdraw_dataset/full/numpy_bitmap/'
    
    for category in categories:
        url = base_url + category + '.npy'
        response = requests.get(url, stream=True)
        with open(f'{category}.npy', 'wb') as f:
            for chunk in response.iter_content(chunk_size=8192):
                f.write(chunk)

# Load and preprocess the data
def load_and_preprocess_data():
    categories = ['circle', 'square', 'zigzag']
    X = []
    y = []
    
    for i, category in enumerate(categories):
        data = np.load(f'{category}.npy')
        # Take first 10000 samples of each category
        data = data[:70000]
        # Reshape to 28x28 images
        data = data.reshape(-1, 28, 28, 1)
        X.append(data)
        y.append(np.full(len(data), i))
    
    X = np.concatenate(X)
    y = np.concatenate(y)
    
    # Normalize pixel values
    X = X.astype('float32') / 255.0
    
    return X, y

# Create and train the model
def create_and_train_model(X, y):
    inputs = keras.Input(shape=(28, 28, 1))
    x = keras.layers.Conv2D(32, (3, 3), activation='relu')(inputs)
    x = keras.layers.MaxPooling2D((2, 2))(x)
    x = keras.layers.Conv2D(64, (3, 3), activation='relu')(x)
    x = keras.layers.MaxPooling2D((2, 2))(x)
    x = keras.layers.Conv2D(64, (3, 3), activation='relu')(x)
    x = keras.layers.Flatten()(x)
    x = keras.layers.Dense(64, activation='relu')(x)
    outputs = keras.layers.Dense(3, activation='softmax')(x)
    model = keras.Model(inputs=inputs, outputs=outputs)

    model.compile(optimizer='adam',
                  loss='sparse_categorical_crossentropy',
                  metrics=['accuracy'])

    model.fit(X, y, epochs=10, validation_split=0.2)

    return model

# Convert to ONNX format
def convert_to_onnx(model):
    import tf2onnx
    
    input_signature = [tf.TensorSpec([1, 28, 28, 1], tf.float32, name="input")]
    onnx_model, _ = tf2onnx.convert.from_keras(model, input_signature, opset=13)
    
    with open("gesture_model.onnx", "wb") as f:
        f.write(onnx_model.SerializeToString())

def main():
    print("Downloading Quick, Draw! data...")
    download_quickdraw_data()
    
    print("Loading and preprocessing data...")
    X, y = load_and_preprocess_data()
    
    print("Creating and training model...")
    model = create_and_train_model(X, y)
    
    print("Converting to ONNX format...")
    convert_to_onnx(model)
    
    print("Done! Model saved as gesture_model.onnx")
    
    # Clean up downloaded files
    for category in ['circle', 'square', 'zigzag']:
        os.remove(f'{category}.npy')

if __name__ == "__main__":
    main() 