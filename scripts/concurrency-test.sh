#!/bin/bash

BASE_URL="https://localhost:7000"
CONCURRENT_REQUESTS=20
PRODUCT_ID=""

# Parse command line arguments
while [[ $# -gt 0 ]]; do
  case $1 in
    --url)
      BASE_URL="$2"
      shift 2
      ;;
    --requests)
      CONCURRENT_REQUESTS="$2"
      shift 2
      ;;
    --product-id)
      PRODUCT_ID="$2"
      shift 2
      ;;
    *)
      echo "Unknown option $1"
      exit 1
      ;;
  esac
done

if [ -z "$PRODUCT_ID" ]; then
    echo "Creating test product first..."
    
    RESPONSE=$(curl -s -X POST "$BASE_URL/api/products" \
        -H "Content-Type: application/json" \
        -d '{
            "name": "Concurrency Test Product",
            "description": "Product for testing concurrent orders",
            "price": 99.99,
            "stockQuantity": 10
        }')
    
    PRODUCT_ID=$(echo $RESPONSE | grep -o '"id":"[^"]*' | cut -d'"' -f4)
    
    if [ -z "$PRODUCT_ID" ]; then
        echo "Failed to create product"
        exit 1
    fi
    
    echo "Created product with ID: $PRODUCT_ID"
fi

echo "Starting $CONCURRENT_REQUESTS concurrent order requests..."

# Create temporary directory for results
TEMP_DIR=$(mktemp -d)
SUCCESS_COUNT=0
FAILURE_COUNT=0

# Start concurrent requests
for i in $(seq 1 $CONCURRENT_REQUESTS); do
    (
        RESPONSE=$(curl -s -w "%{http_code}" -X POST "$BASE_URL/api/orders" \
            -H "Content-Type: application/json" \
            -d "{
                \"orderItems\": [
                    {
                        \"productId\": \"$PRODUCT_ID\",
                        \"quantity\": 1
                    }
                ]
            }")
        
        HTTP_CODE="${RESPONSE: -3}"
        
        if [ "$HTTP_CODE" -eq 201 ]; then
            echo "SUCCESS" > "$TEMP_DIR/result_$i"
        else
            echo "FAILURE: $HTTP_CODE" > "$TEMP_DIR/result_$i"
        fi
    ) &
done

# Wait for all background jobs to complete
wait

# Count results
for file in "$TEMP_DIR"/result_*; do
    if grep -q "SUCCESS" "$file"; then
        ((SUCCESS_COUNT++))
    else
        ((FAILURE_COUNT++))
    fi
done

echo ""
echo "Results:"
echo "Successful orders: $SUCCESS_COUNT"
echo "Failed orders: $FAILURE_COUNT"

# Check final product stock
FINAL_PRODUCT=$(curl -s "$BASE_URL/api/products/$PRODUCT_ID")
FINAL_STOCK=$(echo $FINAL_PRODUCT | grep -o '"stockQuantity":[0-9]*' | cut -d':' -f2)

echo ""
echo "Final product stock: $FINAL_STOCK"
echo "Expected stock: $((10 - SUCCESS_COUNT))"

# Cleanup
rm -rf "$TEMP_DIR"