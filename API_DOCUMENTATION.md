# Post Management API Documentation

## Base URL

```
Development: http://localhost:5203
Production: [Your Production URL]
```

## Overview

This API provides endpoints for managing posts. Each post has a name, description, and an optional image URL.

## Data Models

### Post Response Object
```json
{
  "id": 1,
  "name": "Post Name",
  "description": "Post Description",
  "image": "https://example.com/image.jpg",
  "createdAt": "2025-01-15T10:30:00Z"
}
```

### Create Post Request
```json
{
  "name": "Post Name",
  "description": "Post Description",
  "image": "https://example.com/image.jpg"
}
```

### Update Post Request
```json
{
  "name": "Updated Post Name",
  "description": "Updated Post Description",
  "image": "https://example.com/new-image.jpg"
}
```

## API Endpoints

### 1. Get All Posts

Retrieve a list of all posts with optional search and sort functionality.

**Endpoint:** `GET /api/posts`

**Query Parameters:**
- `search` (optional, string): Search posts by name (case-insensitive partial match)
- `sortOrder` (optional, string): Sort order for posts by name
  - `"asc"` or `"A-Z"`: Sort ascending (A to Z)
  - `"desc"` or `"Z-A"`: Sort descending (Z to A)
  - Default: ascending (A to Z)

**Request Example:**
```http
GET /api/posts?search=example&sortOrder=asc
```

**Response:**
- **Status Code:** `200 OK`
- **Body:**
```json
[
  {
    "id": 1,
    "name": "Example Post",
    "description": "This is an example post",
    "image": "https://example.com/image.jpg",
    "createdAt": "2025-01-15T10:30:00Z"
  },
  {
    "id": 2,
    "name": "Another Post",
    "description": "Another example",
    "image": null,
    "createdAt": "2025-01-15T11:00:00Z"
  }
]
```

**Error Responses:**
- `500 Internal Server Error`: Server error occurred
```json
{
  "message": "An error occurred while retrieving posts"
}
```

---

### 2. Get Post by ID

Retrieve a specific post by its ID.

**Endpoint:** `GET /api/posts/{id}`

**Path Parameters:**
- `id` (required, integer): The ID of the post to retrieve

**Request Example:**
```http
GET /api/posts/1
```

**Response:**
- **Status Code:** `200 OK`
- **Body:**
```json
{
  "id": 1,
  "name": "Example Post",
  "description": "This is an example post",
  "image": "https://example.com/image.jpg",
  "createdAt": "2025-01-15T10:30:00Z"
}
```

**Error Responses:**
- `404 Not Found`: Post not found
```json
{
  "message": "Post with ID 1 not found"
}
```

- `500 Internal Server Error`: Server error occurred
```json
{
  "message": "An error occurred while retrieving the post"
}
```

---

### 3. Create Post

Create a new post.

**Endpoint:** `POST /api/posts`

**Request Headers:**
```
Content-Type: application/json
```

**Request Body:**
```json
{
  "name": "New Post",
  "description": "This is a new post",
  "image": "https://example.com/image.jpg"
}
```

**Field Validation:**
- `name` (required, string, max 200 characters): Post name
- `description` (required, string, max 2000 characters): Post description
- `image` (optional, string, valid URL): Image URL

**Request Example:**
```http
POST /api/posts
Content-Type: application/json

{
  "name": "New Post",
  "description": "This is a new post",
  "image": "https://example.com/image.jpg"
}
```

**Response:**
- **Status Code:** `201 Created`
- **Headers:** `Location: /api/posts/{id}`
- **Body:**
```json
{
  "id": 3,
  "name": "New Post",
  "description": "This is a new post",
  "image": "https://example.com/image.jpg",
  "createdAt": "2025-01-15T12:00:00Z"
}
```

**Error Responses:**
- `400 Bad Request`: Validation errors
```json
{
  "name": ["Name is required"],
  "description": ["Description is required"],
  "image": ["Image must be a valid URL"]
}
```

- `500 Internal Server Error`: Server error occurred
```json
{
  "message": "An error occurred while creating the post"
}
```

---

### 4. Update Post

Update an existing post.

**Endpoint:** `PUT /api/posts/{id}`

**Path Parameters:**
- `id` (required, integer): The ID of the post to update

**Request Headers:**
```
Content-Type: application/json
```

**Request Body:**
```json
{
  "name": "Updated Post Name",
  "description": "Updated description",
  "image": "https://example.com/new-image.jpg"
}
```

**Field Validation:**
- `name` (required, string, max 200 characters): Post name
- `description` (required, string, max 2000 characters): Post description
- `image` (optional, string, valid URL): Image URL

**Request Example:**
```http
PUT /api/posts/1
Content-Type: application/json

{
  "name": "Updated Post Name",
  "description": "Updated description",
  "image": "https://example.com/new-image.jpg"
}
```

**Response:**
- **Status Code:** `200 OK`
- **Body:**
```json
{
  "id": 1,
  "name": "Updated Post Name",
  "description": "Updated description",
  "image": "https://example.com/new-image.jpg",
  "createdAt": "2025-01-15T10:30:00Z"
}
```

**Error Responses:**
- `400 Bad Request`: Validation errors
```json
{
  "name": ["Name is required"],
  "description": ["Description is required"]
}
```

- `404 Not Found`: Post not found
```json
{
  "message": "Post with ID 1 not found"
}
```

- `500 Internal Server Error`: Server error occurred
```json
{
  "message": "An error occurred while updating the post"
}
```

---

### 5. Delete Post

Delete a post by ID.

**Endpoint:** `DELETE /api/posts/{id}`

**Path Parameters:**
- `id` (required, integer): The ID of the post to delete

**Request Example:**
```http
DELETE /api/posts/1
```

**Response:**
- **Status Code:** `204 No Content`
- **Body:** Empty

**Error Responses:**
- `404 Not Found`: Post not found
```json
{
  "message": "Post with ID 1 not found"
}
```

- `500 Internal Server Error`: Server error occurred
```json
{
  "message": "An error occurred while deleting the post"
}
```

---

## Frontend Implementation Examples

### JavaScript/Fetch API

#### Get All Posts with Search and Sort
```javascript
async function getAllPosts(search = '', sortOrder = 'asc') {
  const url = new URL('http://localhost:5203/api/posts');
  if (search) url.searchParams.append('search', search);
  if (sortOrder) url.searchParams.append('sortOrder', sortOrder);
  
  const response = await fetch(url);
  const posts = await response.json();
  return posts;
}
```

#### Get Post by ID
```javascript
async function getPostById(id) {
  const response = await fetch(`http://localhost:5203/api/posts/${id}`);
  if (response.status === 404) {
    throw new Error('Post not found');
  }
  const post = await response.json();
  return post;
}
```

#### Create Post
```javascript
async function createPost(postData) {
  const response = await fetch('http://localhost:5203/api/posts', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(postData),
  });
  
  if (!response.ok) {
    const error = await response.json();
    throw new Error(JSON.stringify(error));
  }
  
  const newPost = await response.json();
  return newPost;
}
```

#### Update Post
```javascript
async function updatePost(id, postData) {
  const response = await fetch(`http://localhost:5203/api/posts/${id}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(postData),
  });
  
  if (!response.ok) {
    const error = await response.json();
    throw new Error(JSON.stringify(error));
  }
  
  const updatedPost = await response.json();
  return updatedPost;
}
```

#### Delete Post
```javascript
async function deletePost(id) {
  const response = await fetch(`http://localhost:5203/api/posts/${id}`, {
    method: 'DELETE',
  });
  
  if (response.status === 404) {
    throw new Error('Post not found');
  }
  
  if (!response.ok) {
    const error = await response.json();
    throw new Error(JSON.stringify(error));
  }
  
  return true; // Successfully deleted
}
```

### Axios Example

```javascript
import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5203/api',
});

// Get all posts
export const getPosts = async (search = '', sortOrder = 'asc') => {
  const response = await api.get('/posts', {
    params: { search, sortOrder }
  });
  return response.data;
};

// Get post by ID
export const getPostById = async (id) => {
  const response = await api.get(`/posts/${id}`);
  return response.data;
};

// Create post
export const createPost = async (postData) => {
  const response = await api.post('/posts', postData);
  return response.data;
};

// Update post
export const updatePost = async (id, postData) => {
  const response = await api.put(`/posts/${id}`, postData);
  return response.data;
};

// Delete post
export const deletePost = async (id) => {
  await api.delete(`/posts/${id}`);
};
```

## Error Handling

All endpoints return appropriate HTTP status codes:
- `200 OK`: Successful GET, PUT requests
- `201 Created`: Successful POST request
- `204 No Content`: Successful DELETE request
- `400 Bad Request`: Validation errors
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server errors

Error responses include a `message` field with details about the error.

## CORS

The API is configured to allow CORS from any origin. This allows frontend applications running on different ports/domains to access the API.

## Testing with Swagger

When running in Development mode, you can access Swagger UI at:
```
http://localhost:5203/swagger
```

This provides an interactive interface to test all API endpoints.

## Notes

1. All timestamps are in UTC format (ISO 8601)
2. Image field is optional and can be `null` if not provided
3. Search is case-insensitive and matches partial names
4. Sort order defaults to ascending (A-Z) if not specified
5. All string fields have maximum length constraints:
   - Name: 200 characters
   - Description: 2000 characters
   - Image URL: 500 characters

