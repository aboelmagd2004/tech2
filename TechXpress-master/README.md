# TechXpress - E-Commerce Platform

TechXpress is a modern e-commerce platform built with ASP.NET Core MVC, designed to provide a seamless shopping experience for technology and electronics products.

## 🔑 Key Features

### Customer Features
- Product catalog with categories and search
- User registration and authentication
- Shopping cart functionality
- Order management and tracking
- Product reviews and ratings
- User profile management
- Secure checkout process

### Admin Features
- Product management
- Category management 
- Order processing
- User management
- Analytics dashboard
- Content management

## 🛠 Technology Stack

### Backend
- ASP.NET Core MVC (.NET 8.0)
- Entity Framework Core
- SQL Server
- Identity Framework for authentication

### Frontend
- HTML5, CSS3, JavaScript
- Bootstrap 5
- jQuery
- Font Awesome icons
- Toastr notifications

### Tools & Libraries
- Data Tables for grid functionality
- CKEditor for rich text editing
- Image handling with file upload

## 📂 Project Structure

```
TechXpress/
├── TechXpress.Data/           # Data access layer
│   ├── Models/                # Entity models
│   ├── Repositories/          # Data repositories
│   └── UnitOfWork/           # Unit of work pattern
├── TechXpress.Services/       # Business logic layer
│   ├── CartItemsService/
│   ├── OrdersService/
│   └── ProductsService/
└── TechXpress.Web/           # Presentation layer
    ├── Controllers/
    ├── Models/
    ├── Views/
    └── wwwroot/
```

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server
- Visual Studio 2022 or VS Code

### Installation
1. Clone the repository
```bash
git clone https://github.com/eng-yossef/TechXpress
```

2. Update connection string in `appsettings.json`

3. Run migrations
```bash
dotnet ef database update
```

4. Run the application
```bash
dotnet run
```

## 📦 Features in Detail

### User Management
- Custom user model with profile information
- Email verification
- Password recovery
- Two-factor authentication support
- Profile picture management

### Product Management
- Rich product details
- Multiple product images
- Category organization
- Product specifications
- Stock management

### Shopping Experience
- Advanced search functionality
- Product filtering
- Shopping cart
- Wishlist
- Related products

### Order System
- Multiple payment methods
- Order tracking
- Email notifications
- Invoice generation
- Shipping integration

## 🔒 Security Features

- HTTPS enforcement
- Cross-Site Request Forgery (CSRF) protection
- SQL injection prevention
- Password hashing
- Role-based authorization

## 📱 Responsive Design

- Mobile-first approach
- Responsive navigation
- Touch-friendly interfaces
- Optimized images
- Flexible layouts

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch
3. Commit your changes
4. Push to the branch
5. Open a Pull Request

## ⚙️ Configuration

Key configuration files:
- `appsettings.json`: Application settings
- Program.cs: Application startup
- _Layout.cshtml: Main layout template

## 📄 License

This project is licensed under the MIT License

## 📧 Contact

For support or queries, please contact:
- Email: yossefkhaled551@gmail.com
- Phone: 01150648044

---

Built with ❤️ by the TechXpress Team
