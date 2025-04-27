import pickle
import base64

# Example test data (like a ResFile)
data = {
    "key": "value"
}

# Pickle the data
pickled_data = pickle.dumps(data)

# Encode to base64
b64_encoded = base64.b64encode(pickled_data).decode('utf-8')

# Print the base64 string
print(b64_encoded)
