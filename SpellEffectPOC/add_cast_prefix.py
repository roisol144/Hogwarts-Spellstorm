import os

# Set this to your Resources folder path
folder = '/Users/roisolomon/Documents/DEV/Hogwarts-Spellstorm/SpellEffectPOC/Assets/Resources/Gestures/'

# Add 'cast_' prefix to .xml files that don't have it
for filename in os.listdir(folder):
    if filename.endswith('.xml') and not filename.startswith('cast_'):
        old_path = os.path.join(folder, filename)
        new_filename = 'cast_' + filename
        new_path = os.path.join(folder, new_filename)
        os.rename(old_path, new_path)
        print(f'Renamed: {filename} -> {new_filename}')

# Delete all .meta files
for filename in os.listdir(folder):
    if filename.endswith('.meta'):
        meta_path = os.path.join(folder, filename)
        os.remove(meta_path)
        print(f'Deleted: {filename}')