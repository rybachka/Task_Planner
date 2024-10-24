document.addEventListener("DOMContentLoaded", async function () {
    try {
        console.log('Attempting to fetch projects...');
        const response = await fetch('/api/projects');
        
        // Sprawdzenie, czy odpowiedź z serwera jest OK (status 200)
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        console.log('Data fetched:', data);

        const projectList = document.getElementById('project-list');
        data.forEach(project => {
            const projectElement = document.createElement('div');
            projectElement.innerHTML = `<h3>${project.name}</h3><p>${project.description}</p>`;
            projectList.appendChild(projectElement);
        });
    } catch (error) {
        console.error('Error fetching data:', error);
    }
});
