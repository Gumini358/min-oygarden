
console.log("Script loaded");

// Adresse til backend på Render
const API_URL = "https://min-oygarden.onrender.com";

// Skjema for meldinger
const formMessage = document.querySelector("#form-message");
const reportform = document.querySelector(".report-form");

if (reportform) {
    reportform.addEventListener("submit", (event) => {
        event.preventDefault();
        reportform.reset();

        if (formMessage) {
            formMessage.textContent = "Meldingen din er sendt!";

            setTimeout(() => {
                formMessage.textContent = "";
            }, 3000);
        }
    });
}

// Søk etter tjenester
const serviceSearchInput = document.querySelector(
    ".service-search input"
);

if (serviceSearchInput) {
    serviceSearchInput.addEventListener("input", (event) => {
        const noResultsMessage = document.querySelector(
            "#no-results"
        );

        const searchTerm = event.target.value.toLowerCase();
        const serviceItems = document.querySelectorAll(
            ".service-item"
        );

        serviceItems.forEach((item) => {
            const serviceName = item
                .querySelector(".service-card")
                .textContent.toLowerCase();

            item.style.display = serviceName.includes(searchTerm)
                ? "flex"
                : "none";
        });

        if (noResultsMessage) {
            const anyVisible = Array.from(serviceItems).some(
                (item) => item.style.display !== "none"
            );

            noResultsMessage.style.display = anyVisible
                ? "none"
                : "block";
        }
    });
}

// Mobilmeny
const menuToggle = document.querySelector(".menu-toggle");
const navbar = document.querySelector(".navbar");

if (menuToggle && navbar) {
    menuToggle.addEventListener("click", () => {
        navbar.classList.toggle("open");
    });
}

// Eksisterende vurderingsknapper
const ratings = document.querySelectorAll(".rating");

ratings.forEach((rating) => {
    const stars = rating.querySelectorAll("button");

    stars.forEach((button) => {
        button.addEventListener("click", () => {
            const selectedValue = Number(button.dataset.value);

            stars.forEach((star) => {
                const starValue = Number(star.dataset.value);

                star.textContent = starValue <= selectedValue
                    ? "★"
                    : "☆";
            });
        });
    });
});

// Henter tjenester fra PostgreSQL via Render
async function getData() {
    try {
        const response = await fetch(
            `${API_URL}/api/tjenester`
        );

        if (!response.ok) {
            throw new Error(
                `HTTP-feil ved henting av tjenester: ${response.status}`
            );
        }

        const data = await response.json();

        const tjenesteSeksjon = document.getElementById(
            "tjenester"
        );

        const servicesContainer = tjenesteSeksjon?.querySelector(
            ".services-container"
        );

        if (!servicesContainer) {
            console.error("Fant ikke tjenestebeholderen.");
            return data;
        }

        servicesContainer.innerHTML = "";

        data.forEach((tjeneste) => {
            const nyttElement = document.createElement("div");

            nyttElement.classList.add("service-item");

            nyttElement.setAttribute("data-id", tjeneste.id);
            nyttElement.setAttribute(
                "data-name",
                tjeneste.navn
            );
            nyttElement.setAttribute(
                "data-description",
                tjeneste.beskrivelse
            );
            nyttElement.setAttribute(
                "data-vurdering",
                tjeneste.vurdering
            );
            nyttElement.setAttribute(
                "data-kategori",
                tjeneste.kategori
            );

            // Navn og tjenestekort
            const nameElement = document.createElement("h3");
            nameElement.textContent = tjeneste.navn;

            const serviceCard = document.createElement("a");
            serviceCard.classList.add("service-card");
            serviceCard.href = "#";
            serviceCard.appendChild(nameElement);

            nyttElement.appendChild(serviceCard);

            // Beskrivelse
            const descriptionElement = document.createElement("p");
            descriptionElement.classList.add(
                "service-description"
            );
            descriptionElement.textContent =
                tjeneste.beskrivelse;

            nyttElement.appendChild(descriptionElement);

            // Vurdering med stjerner
            const ratingElement = document.createElement("div");
            ratingElement.classList.add("rating");

            let gjeldendeVurdering = Number(
                tjeneste.vurdering
            ) || 0;

            function oppdaterStjerner(verdi) {
                const stars = ratingElement.querySelectorAll(
                    "button"
                );

                stars.forEach((star) => {
                    const starValue = Number(
                        star.dataset.value
                    );

                    star.textContent = starValue <= verdi
                        ? "★"
                        : "☆";
                });
            }

            for (let i = 1; i <= 5; i++) {
                const starButton = document.createElement(
                    "button"
                );

                starButton.type = "button";
                starButton.dataset.value = i;

                starButton.textContent =
                    i <= gjeldendeVurdering ? "★" : "☆";

                starButton.addEventListener(
                    "click",
                    async () => {
                        const selectedValue = Number(
                            starButton.dataset.value
                        );

                        try {
                            const response = await fetch(
                                `${API_URL}/api/tjenester/${tjeneste.id}/vurdering/${selectedValue}`,
                                {
                                    method: "POST"
                                }
                            );

                            if (!response.ok) {
                                throw new Error(
                                    `HTTP-feil: ${response.status}`
                                );
                            }

                            gjeldendeVurdering = selectedValue;
                            oppdaterStjerner(selectedValue);

                            console.log(
                                "Vurdering lagret:",
                                tjeneste.navn,
                                selectedValue
                            );
                        } catch (error) {
                            console.error(
                                "Kunne ikke lagre vurderingen:",
                                error
                            );

                            oppdaterStjerner(
                                gjeldendeVurdering
                            );

                            alert(
                                "Kunne ikke lagre vurderingen."
                            );
                        }
                    }
                );

                ratingElement.appendChild(starButton);
            }

            nyttElement.appendChild(ratingElement);

            // Lenke til tjenesten
            if (tjeneste.lenke) {
                const linkElement = document.createElement(
                    "a"
                );

                linkElement.href = tjeneste.lenke;
                linkElement.textContent = "Les mer";
                linkElement.target = "_blank";
                linkElement.rel = "noopener noreferrer";

                nyttElement.appendChild(linkElement);
            }

            // Bilde
            if (tjeneste.bilde) {
                const imageElement = document.createElement(
                    "img"
                );

                imageElement.src = tjeneste.bilde;
                imageElement.alt = tjeneste.navn;
                imageElement.classList.add("service-image");

                nyttElement.appendChild(imageElement);
            }

            // Kategori
            const categoryElement = document.createElement("p");
            categoryElement.classList.add(
                "service-category"
            );

            categoryElement.textContent =
                `Kategori: ${tjeneste.kategori}`;

            nyttElement.appendChild(categoryElement);

            servicesContainer.appendChild(nyttElement);
        });

        return data;
    } catch (error) {
        console.error(
            "Kunne ikke hente tjenester:",
            error
        );

        return [];
    }
}

// Administrator
const adminLoginForm = document.getElementById(
    "admin-login-form"
);

const adminMessage = document.getElementById(
    "admin-message"
);

const adminLogoutButton = document.getElementById(
    "admin-logout"
);

let adminToken = null;

// Kommentarer
const kommentarForm = document.getElementById(
    "kommentar-form"
);

const kommentarListe = document.getElementById(
    "kommentar-liste"
);

const kommentarAntall = document.getElementById(
    "kommentar-antall"
);

// Henter kommentarer fra PostgreSQL
async function hentKommentarer() {
    try {
        const response = await fetch(
            `${API_URL}/api/kommentarer`
        );

        if (!response.ok) {
            throw new Error(
                `HTTP-feil ved henting av kommentarer: ${response.status}`
            );
        }

        const kommentarer = await response.json();

        if (kommentarAntall) {
            kommentarAntall.textContent =
                `Kommentarer (${kommentarer.length})`;
        }

        console.log(
            "Lagrede kommentarer:",
            kommentarer.length
        );

        if (!kommentarListe) {
            return;
        }

        kommentarListe.innerHTML = "";

        // Sorterer kommentarene etter dato
        kommentarer.sort((a, b) => {
            const datoA = a.dato
                ? new Date(a.dato.replace(" ", "T"))
                : new Date(0);

            const datoB = b.dato
                ? new Date(b.dato.replace(" ", "T"))
                : new Date(0);

            return datoB - datoA;
        });

        kommentarer.forEach((kommentar) => {
            const kommentarElement = document.createElement(
                "div"
            );

            kommentarElement.classList.add("kommentar");

            // Navn
            const navnElement = document.createElement(
                "strong"
            );

            navnElement.textContent = kommentar.navn;

            // Tekst
            const tekstElement = document.createElement("p");
            tekstElement.textContent = kommentar.tekst;

            // Dato
            const datoElement = document.createElement("p");

            datoElement.textContent = kommentar.dato ||
                "Dato ikke tilgjengelig";

            datoElement.classList.add("kommentar-dato");

            kommentarElement.appendChild(navnElement);
            kommentarElement.appendChild(datoElement);
            kommentarElement.appendChild(tekstElement);

            // Sletteknapp for administrator
            if (adminToken && kommentar.id) {
                const slettKnapp = document.createElement(
                    "button"
                );

                slettKnapp.type = "button";
                slettKnapp.textContent = "Slett kommentar";

                slettKnapp.classList.add(
                    "delete-comment-btn"
                );

                slettKnapp.addEventListener(
                    "click",
                    async () => {
                        const bekreftet = confirm(
                            "Vil du slette denne kommentaren?"
                        );

                        if (!bekreftet) {
                            return;
                        }

                        try {
                            const response = await fetch(
                                `${API_URL}/api/kommentarer/${encodeURIComponent(kommentar.id)}`,
                                {
                                    method: "DELETE",
                                    headers: {
                                        Authorization:
                                            `Bearer ${adminToken}`
                                    }
                                }
                            );

                            if (!response.ok) {
                                throw new Error(
                                    `HTTP-feil: ${response.status}`
                                );
                            }

                            await hentKommentarer();
                        } catch (error) {
                            console.error(
                                "Kunne ikke slette kommentaren:",
                                error
                            );

                            alert(
                                "Kunne ikke slette kommentaren."
                            );
                        }
                    }
                );

                kommentarElement.appendChild(slettKnapp);
            }

            kommentarListe.appendChild(kommentarElement);
        });
    } catch (error) {
        console.error(
            "Kunne ikke hente kommentarer:",
            error
        );

        if (kommentarAntall) {
            kommentarAntall.textContent =
                "Kunne ikke hente kommentarer";
        }
    }
}

// Legger til en ny kommentar
if (kommentarForm) {
    kommentarForm.addEventListener(
        "submit",
        async (event) => {
            event.preventDefault();

            const navn = document.getElementById(
                "kommentar-navn"
            ).value.trim();

            const tekst = document.getElementById(
                "kommentar-tekst"
            ).value.trim();

            try {
                const response = await fetch(
                    `${API_URL}/api/kommentarer`,
                    {
                        method: "POST",
                        headers: {
                            "Content-Type": "application/json"
                        },
                        body: JSON.stringify({
                            navn,
                            tekst
                        })
                    }
                );

                if (!response.ok) {
                    throw new Error(
                        `HTTP-feil: ${response.status}`
                    );
                }

                kommentarForm.reset();

                const kommentarTegn = document.getElementById(
                    "kommentar-tegn"
                );

                if (kommentarTegn) {
                    kommentarTegn.textContent =
                        "0 / 500 tegn";
                }

                await hentKommentarer();

                console.log(
                    "Kommentar lagret i PostgreSQL."
                );
            } catch (error) {
                console.error(
                    "Feil ved innsending av kommentar:",
                    error
                );

                alert(
                    "Kunne ikke sende kommentaren."
                );
            }
        }
    );
}

// Logg inn administratoren
if (adminLoginForm) {
    adminLoginForm.addEventListener(
        "submit",
        async (event) => {
            event.preventDefault();

            const password = document.getElementById(
                "admin-password"
            ).value;

            try {
                const response = await fetch(
                    `${API_URL}/api/admin/login`,
                    {
                        method: "POST",
                        headers: {
                            "Content-Type": "application/json"
                        },
                        body: JSON.stringify({
                            password
                        })
                    }
                );

                if (response.ok) {
                    const data = await response.json();

                    adminToken = data.token;

                    await hentKommentarer();

                    if (adminMessage) {
                        adminMessage.textContent =
                            "Du er logget inn som administrator!";
                    }

                    adminLoginForm.reset();

                    if (adminLogoutButton) {
                        adminLogoutButton.hidden = false;
                    }
                } else {
                    adminToken = null;

                    if (adminMessage) {
                        adminMessage.textContent =
                            "Feil passord!";
                    }
                }
            } catch (error) {
                adminToken = null;

                if (adminMessage) {
                    adminMessage.textContent =
                        "Kunne ikke koble til serveren.";
                }

                console.error(error);
            }
        }
    );
}

// Logg ut administratoren
if (adminLogoutButton) {
    adminLogoutButton.addEventListener(
        "click",
        async () => {
            adminToken = null;

            if (adminMessage) {
                adminMessage.textContent =
                    "Du er logget ut!";
            }

            adminLogoutButton.hidden = true;

            await hentKommentarer();
        }
    );
}

// Teller antall tegn i kommentaren
const kommentarTekst = document.getElementById(
    "kommentar-tekst"
);

const kommentarTegn = document.getElementById(
    "kommentar-tegn"
);

if (kommentarTekst && kommentarTegn) {
    kommentarTekst.addEventListener("input", () => {
        const antall = kommentarTekst.value.length;

        kommentarTegn.textContent =
            `${antall} / 500 tegn`;
    });
}

// Starter innlasting av tjenester og kommentarer
getData();
hentKommentarer();
