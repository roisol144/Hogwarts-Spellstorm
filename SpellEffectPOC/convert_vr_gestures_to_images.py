import xml.etree.ElementTree as ET
import numpy as np
import cv2
import os
from tqdm import tqdm

# --- CONFIGURATION ---
# Set these paths before running
XML_DIR = '/Users/roisolomon/Downloads/GesturesRecordings'  # Directory containing your extracted XML files
OUT_IMAGE_DIR = 'gesture_images'     # Directory to save PNG images for inspection
OUT_NPY_IMAGES = 'vr_gesture_images.npy'  # Output numpy file for images
OUT_NPY_LABELS = 'vr_gesture_labels.npy'  # Output numpy file for labels
IMG_SIZE = 28

# --- UTILITY FUNCTIONS ---
def xml_to_points(xml_file):
    tree = ET.parse(xml_file)
    root = tree.getroot()
    points = []
    for point in root.find('Stroke').findall('Point'):
        x = float(point.get('X'))
        y = float(point.get('Y'))
        points.append((x, y))
    return np.array(points)

def points_to_image(points, size=28):
    if len(points) < 2:
        return np.zeros((size, size), dtype=np.uint8)
    # Normalize and center points
    min_xy = points.min(axis=0)
    max_xy = points.max(axis=0)
    scale = (size - 2) / max((max_xy - min_xy).max(), 1e-5)
    points = (points - min_xy) * scale + 1  # Padding of 1 pixel
    img = np.zeros((size, size), dtype=np.uint8)
    for i in range(1, len(points)):
        pt1 = tuple(np.clip(points[i-1].astype(int), 0, size-1))
        pt2 = tuple(np.clip(points[i].astype(int), 0, size-1))
        cv2.line(img, pt1, pt2, 255, 1)
    return img

# --- MAIN SCRIPT ---
def main():
    os.makedirs(OUT_IMAGE_DIR, exist_ok=True)
    images = []
    labels = []
    label_map = {}
    label_counter = 0

    xml_files = [f for f in os.listdir(XML_DIR) if f.endswith('.xml')]
    print(f"Found {len(xml_files)} XML files.")

    for fname in tqdm(xml_files):
        xml_path = os.path.join(XML_DIR, fname)
        # Label: try to get from filename or XML attribute
        try:
            tree = ET.parse(xml_path)
            root = tree.getroot()
            label = root.attrib.get('Name')
            if not label:
                label = fname.split('_')[0]
        except Exception as e:
            print(f"Error parsing {fname}: {e}")
            continue
        if label not in label_map:
            label_map[label] = label_counter
            label_counter += 1
        label_idx = label_map[label]
        # Points to image
        points = xml_to_points(xml_path)
        img = points_to_image(points, IMG_SIZE)
        images.append(img)
        labels.append(label_idx)
        # Save PNG for inspection
        out_png = os.path.join(OUT_IMAGE_DIR, f"{os.path.splitext(fname)[0]}.png")
        cv2.imwrite(out_png, img)

    images = np.array(images, dtype=np.uint8)
    labels = np.array(labels, dtype=np.int64)
    print(f"Saving {len(images)} images and {len(labels)} labels...")
    np.save(OUT_NPY_IMAGES, images)
    np.save(OUT_NPY_LABELS, labels)
    print("Label map:", label_map)
    print("Done! You can now use these .npy files for model training.")

if __name__ == "__main__":
    main()

"""
INSTRUCTIONS:
1. Set XML_DIR to the folder where you extracted your VR gesture XML files from the headset.
2. Run this script: python convert_vr_gestures_to_images.py
3. The script will create:
   - gesture_images/ : PNGs of each gesture for visual inspection
   - vr_gesture_images.npy : Numpy array of shape (N, 28, 28) with all gesture images
   - vr_gesture_labels.npy : Numpy array of shape (N,) with integer labels
   - Prints the label map (int to gesture name)
4. Use these .npy files to fine-tune your model in Keras or PyTorch.
""" 