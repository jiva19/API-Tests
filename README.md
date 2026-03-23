# 🚀 API Automation Suite: RESTSharp & Testcontainers

This repository demonstrates a professional approach to API integration testing using **C#**, **RESTSharp**, and **Testcontainers**. The project focuses on validating a custom Order Controller with advanced scenarios including Role-Based Access Control (RBAC), database isolation, and concurrency.

---

## 🛠️ Project Architecture: Isolated Integration Testing
Instead of testing against a "live" or "staged" environment, this suite uses a **Custom Test Web Server** and a **SQL Testcontainer**. This ensures that every test run is 100% isolated, reproducible, and does not leave "trash" data in a real database.

### 🔌 Key Technical Features:
* **Containerized Database:** Integrated **Testcontainers for .NET** to spin up a real SQL Server instance for the duration of the test suite.
* **Custom WebApplicationFactory:** Leveraged a custom test server to inject authentication mocks and middleware for deep integration testing.
* **RESTSharp Implementation:** Built a robust API client using RESTSharp to handle request construction, header management, and response deserialization.

---

## 🧪 Test Coverage & Scenarios
The suite covers the full CRUD lifecycle of an `OrderController` (GET, POST, PUT, DELETE) with a focus on edge cases:

* **Authentication & RBAC:** Validated Role-Based Authorization by testing different JWT/Claims scenarios (Admin vs. User access).
* **HTTP Status & Schema:** Asserted correct status codes (200 OK, 201 Created, 204 No Content) and validated the integrity of JSON responses.
* **Security & Validation:** Verified **401 Unauthorized** and **403 Forbidden** responses, as well as **400 Bad Request** for invalid payloads.
* **Concurrency Testing:** Executed parallel requests to ensure the API and database handle race conditions and simultaneous transactions correctly.
* **Error Handling:** Tested the system's resilience against malformed data and boundary-value inputs.

---

### 🧰 Tech Stack
* **Language:** C# / .NET
* **API Client:** RESTSharp
* **Database:** SQL Server (via Testcontainers)
* **Testing Framework:** xUnit / NUnit (specify your choice here)
* **Server Mocking:** Microsoft.AspNetCore.Mvc.Testing

---

### 💡 Why this approach?
By using a **Custom Test Server** and **SQL Containers**, this project eliminates the "flaky test" problem. The environment is created, migrated, and destroyed automatically, allowing for high-speed testing in a local environment or a CI/CD pipeline.
