

console.log('Script loaded');

const formMessage = document.querySelector('#form-message');
const reportform = document.querySelector('.report-form');

if (reportform) {
    reportform.addEventListener('submit', (event) => {
        event.preventDefault();
        reportform.reset();
        if (formMessage) {
            formMessage.textContent = 'Meldingen din er sendt!';
            setTimeout(() => {
                formMessage.textContent = '';
            }, 3000);
        }
    });
}

const serviceSearchInput = document.querySelector('.service-search input');
if (serviceSearchInput) {
    serviceSearchInput.addEventListener('input', (event) => {
        const noResultsMessage = document.querySelector('#no-results');
        const searchTerm = event.target.value.toLowerCase();
        const serviceItems = document.querySelectorAll('.service-item');
        serviceItems.forEach((item) => {
            const serviceName = item.querySelector('.service-card').textContent.toLowerCase();
            if (serviceName.includes(searchTerm)) {
                item.style.display = 'flex';
            } else {
                item.style.display = 'none';
            }
        });
        if (noResultsMessage) {
            const anyVisible = Array.from(serviceItems).some(item => item.style.display !== 'none');
            noResultsMessage.style.display = anyVisible ? 'none' : 'block';
        }
    });
}

const menuToggle = document.querySelector(".menu-toggle");
const navbar = document.querySelector(".navbar");

menuToggle.addEventListener("click", () => {
    navbar.classList.toggle("open");
});

const ratings = document.querySelectorAll(".rating");

ratings.forEach((rating) => {
    const stars = rating.querySelectorAll("button");

    stars.forEach((button) => {
        button.addEventListener("click", () => {
            const selectedValue = Number(button.dataset.value);

            stars.forEach((star) => {
                const starValue = Number(star.dataset.value);

                if (starValue <= selectedValue) {
                    star.textContent = "★";
                } else {
                    star.textContent = "☆";
                }
            });
        });
    });
});

async function getData() {
    const url = "http://localhost:5159/api/tjenester";
    try {
        const response = await fetch(url);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const tjenesteSeksjon = document.getElementById("tjenester");
        const servicesContainer = tjenesteSeksjon.querySelector(".services-container");
        const data = await response.json();
  data.forEach((tjeneste) => {
    const nyttElement = document.createElement("div");
    nyttElement.classList.add("service-item");
  

    nyttElement.setAttribute("data-id", tjeneste.id);
    nyttElement.setAttribute("data-name", tjeneste.navn);
    nyttElement.setAttribute("data-description", tjeneste.beskrivelse);
    nyttElement.setAttribute("data-vurdering", tjeneste.vurdering);
    nyttElement.setAttribute("data-kategori", tjeneste.kategori);
    

    const nameElement = document.createElement("h3");
    nameElement.textContent = tjeneste.navn;
    const serviceCard = document.createElement("a");
    serviceCard.classList.add("service-card");
    serviceCard.href = "#";
    serviceCard.appendChild(nameElement);
    nyttElement.appendChild(serviceCard);
    // nyttElement.appendChild(nameElement);

    const descriptionElement = document.createElement("p");
    descriptionElement.classList.add("service-description");
    descriptionElement.textContent = tjeneste.beskrivelse;
    nyttElement.appendChild(descriptionElement);

    const ratingElement = document.createElement("div");
ratingElement.classList.add("rating");

for (let i = 1; i <= 5; i++) {
    const starButton = document.createElement("button");
    starButton.type = "button";
    starButton.dataset.value = i;
    starButton.textContent = i <= tjeneste.vurdering ? "★" : "☆";
    starButton.addEventListener("click", async () => {
    const selectedValue = Number(starButton.dataset.value);

   const response = await fetch(
    `http://localhost:5159/api/tjenester/${tjeneste.id}/vurdering/${selectedValue}`,
    {
        method: "POST"
    }
);

console.log("Status zapisu:", response.status);

    const stars = ratingElement.querySelectorAll("button");

    stars.forEach((star) => {
        const starValue = Number(star.dataset.value);

        if (starValue <= selectedValue) {
            star.textContent = "★";
        } else {
            star.textContent = "☆";
        }
    });
});

    ratingElement.appendChild(starButton);
}

nyttElement.appendChild(ratingElement);
// Link "Les mer"
if (tjeneste.lenke) {
    const linkElement = document.createElement("a");
    linkElement.href = tjeneste.lenke;
    linkElement.textContent = "Les mer";
    linkElement.target = "_blank";
    linkElement.rel = "noopener noreferrer";

    nyttElement.appendChild(linkElement);
}

// Bilde
if (tjeneste.bilde) {
    const imageElement = document.createElement("img");
    imageElement.src = tjeneste.bilde;
    imageElement.alt = tjeneste.navn;
    imageElement.classList.add("service-image");

    nyttElement.appendChild(imageElement);
}
    const categoryElement = document.createElement("p");
    categoryElement.classList.add("service-category");
    categoryElement.textContent = `Kategori: ${tjeneste.kategori}`;
    nyttElement.appendChild(categoryElement);

    servicesContainer.appendChild(nyttElement);

});
        return data;
    } catch (error) {
        console.error('Error fetching data:', error);
        return [];
    }
}

getData().then(data => console.log(data));

const kommentarForm = document.getElementById("kommentar-form");

kommentarForm.addEventListener("submit", async (event) => {
    event.preventDefault();

    const navn = document.getElementById("kommentar-navn").value;
    const tekst = document.getElementById("kommentar-tekst").value;

    console.log("Navn:", navn);
    console.log("Kommentar:", tekst);

    const response = await fetch("http://localhost:5159/api/kommentarer", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ navn, tekst })
    });

  
if (response.ok) {
    await hentKommentarer();
    kommentarForm.reset();

    const kommentarTegn = document.getElementById("kommentar-tegn");

    if (kommentarTegn) {
        kommentarTegn.textContent = "0 / 500 tegn";
    }
}
 else {
        console.error("Feil ved innsending av kommentar");
    }
});
    async function hentKommentarer() {
    const response = await fetch("http://localhost:5159/api/kommentarer");

    const kommentarer = await response.json();

const kommentarAntall = document.getElementById("kommentar-antall");

if (kommentarAntall) {
    kommentarAntall.textContent = `Kommentarer (${kommentarer.length})`;
}
    console.log("Lagrede kommentarer:", kommentarer);
    const kommentarListe = document.getElementById("kommentar-liste");

kommentarListe.innerHTML = "";
kommentarer.sort((a, b) => {
    const datoA = a.dato ? new Date(a.dato.replace(" ", "T")) : new Date(0);
    const datoB = b.dato ? new Date(b.dato.replace(" ", "T")) : new Date(0);

    return datoB - datoA;
});
kommentarer.forEach((kommentar) => {
    const kommentarElement = document.createElement("div");
    kommentarElement.classList.add("kommentar");

    const navnElement = document.createElement("strong");
    navnElement.textContent = kommentar.navn;

    const tekstElement = document.createElement("p");
    tekstElement.textContent = kommentar.tekst;
    
    const datoElement = document.createElement("p");
    datoElement.textContent = kommentar.dato || "Dato ikke tilgjengelig";
    datoElement.classList.add("kommentar-dato");

   kommentarElement.appendChild(navnElement);
kommentarElement.appendChild(datoElement);
kommentarElement.appendChild(tekstElement);
if (adminToken && kommentar.id) {
    const slettKnapp = document.createElement("button");
    slettKnapp.type = "button";
    slettKnapp.textContent = "Slett kommentar";

slettKnapp.classList.add("delete-comment-btn");
    slettKnapp.addEventListener("click", async () => {
        const bekreftet = confirm("Vil du slette denne kommentaren?");

        if (!bekreftet) return;

        const response = await fetch(
            `http://localhost:5159/api/kommentarer/${encodeURIComponent(kommentar.id)}`,
            {
                method: "DELETE",
                headers: {
                    Authorization: `Bearer ${adminToken}`
                }
            }
        );

        if (response.ok) {
            await hentKommentarer();
        } else {
            alert("Kunne ikke slette kommentaren.");
        }
    });

    kommentarElement.appendChild(slettKnapp);
}
    kommentarListe.appendChild(kommentarElement);
    });
   
}

hentKommentarer();
// Logowanie administratora
const adminLoginForm = document.getElementById("admin-login-form");
const adminMessage = document.getElementById("admin-message");

let adminToken = null;

if (adminLoginForm) {
    adminLoginForm.addEventListener("submit", async (event) => {
        event.preventDefault();

        const password = document.getElementById("admin-password").value;

        try {
            const response = await fetch(
                "http://localhost:5159/api/admin/login",
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify({ password })
                }
            );

            if (response.ok) {
                const data = await response.json();

                adminToken = data.token;
                await hentKommentarer();
                adminMessage.textContent = "Du er logget inn som administrator!";
                adminLoginForm.reset();
                adminLogoutButton.hidden = false;
            } else {
                adminToken = null;
                adminMessage.textContent = "Feil passord!";
            }
        } catch (error) {
            adminToken = null;
            adminMessage.textContent = "Kunne ikke koble til serveren.";
            console.error(error);
        }
    });
}

const adminLogoutButton = document.getElementById("admin-logout");

if (adminLogoutButton) {
    adminLogoutButton.addEventListener("click", async () => {
        adminToken = null;

        adminMessage.textContent = "Du er logget ut!";
        adminLogoutButton.hidden = true;

        await hentKommentarer();
    });
}

const kommentarTekst = document.getElementById("kommentar-tekst");
const kommentarTegn = document.getElementById("kommentar-tegn");

if (kommentarTekst && kommentarTegn) {
    kommentarTekst.addEventListener("input", () => {
        const antall = kommentarTekst.value.length;

        kommentarTegn.textContent = `${antall} / 500 tegn`;
    });
}