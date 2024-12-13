# Grøn & Olsen Auction House

Dette er en microservice-baseret demoapplikation for digitalisering af et auktionshus, udviklet som en del af et projekt. Systemet består af flere microservices, herunder CatalogService, der håndterer produktregistrering og administration.

## 🛠️ Funktionalitet
- **Produktstyring**: Tilføj, opdater, hent og slet produkter.
- **Auktionsforberedelse**: Forbered produkter til auktioner og administrer deres status.
- **Sikkerhed**: Integration med Vault for hemmelighedshåndtering og JWT-baseret autentifikation.
- **Rollestyring**: Adgang baseret på roller (brugere og administratorer).

## 🗂️ Projektstruktur
```
GronOgOlsen
├── AuthService/       # Microservice til autentifikation og JWT-håndtering
├── CatalogService/    # Microservice til produkt- og kataloghåndtering
│   ├── CatalogServiceAPI/ # Indeholder API-implementationen
│   ├── CatalogService.Tests/ # Unit tests for CatalogService
├── GO_Infrastructure/ # Infrastruktur (RabbitMQ, Vault, MongoDB, mv.)
├── GO_POC/            # Proof of Concept-kode for projektet
└── UserService/       # Microservice til brugerstyring
```

## 📚 CatalogService
CatalogService API'en tilbyder endpoints til at håndtere produkter i databasen.

### Endpoints
- **`GET /api/catalog/products`**: Hent alle produkter (kun for administratorer).
- **`GET /api/catalog/product/{id}`**: Hent specifikt produkt (for brugere og administratorer).
- **`POST /api/catalog/product`**: Tilføj nyt produkt.
- **`PUT /api/catalog/product/{id}`**: Opdater et produkt (kun for administratorer).
- **`DELETE /api/catalog/product/{id}`**: Slet et produkt (kun for administratorer).

## 🔒 Sikkerhed
- **Vault Integration**: Hemmeligheder som JWT-signeringsnøgler og databaseforbindelser hentes fra HashiCorp Vault.
- **JWT Authentication**: Beskyttelse af endpoints baseret på roller:
  - **Bruger**: Kan se og oprette produkter.
  - **Administrator**: Kan administrere alle produkter.

## 🧪 Test
- **Unit tests**: Bruger MSTest og Moq til at teste controllerens funktionalitet.
- Eksempel på test:
  - `GetAllProducts_ReturnsOkWithProducts()`: Tester, om alle produkter hentes korrekt.

### Sådan kører du tests
1. Naviger til testmappen:
   ```bash
   cd CatalogService/CatalogService.Tests
   ```
2. Kør tests med:
   ```bash
   dotnet test
   ```

## 🚀 Kom i gang
### Krav
- **.NET 8.0**
- **Docker**
- **HashiCorp Vault**
- **MongoDB**

### Opsætning
1. Start infrastrukturen med Docker Compose:
   ```bash
   docker-compose -f GO_Infrastructure/docker-compose.yml up -d
   ```
2. Naviger til en microservice, f.eks. `CatalogService`:
   ```bash
   cd CatalogService/CatalogServiceAPI
   ```
3. Start API'en:
   ```bash
   dotnet run
   ```

### Eksempel på miljøvariabler
```env
Vault__Address=http://localhost:8200
Vault__Token=my-vault-token
DatabaseName=CatalogDB
```

## 📧 Kontakt
Hvis du har spørgsmål eller forslag, så kontakt teamet bag Grøn & Olsen!
