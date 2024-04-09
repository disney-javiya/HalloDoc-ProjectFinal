
function pagination() {

    const cardsPerPage = 10; // Number of cards to show per page
    const dataContainer = document.querySelector('.admin-table');
    const pagination = document.getElementById('pagination');
    const prevButton = document.getElementById('prev');
    const nextButton = document.getElementById('next');
    const pageNumbers = document.getElementById('page-numbers');
    const pageLinks = document.querySelectorAll('.page-link');

    const cardss =
        document.querySelectorAll('#BackgroundColor');

    const totalPages = Math.ceil(cardss.length / cardsPerPage);
    let currentPage = 1;



    function displayPage(page) {
        const startIndex = (page - 1) * cardsPerPage;
        const endIndex = startIndex + cardsPerPage;
        const rows = document.querySelectorAll('.admin-table tbody tr');

        rows.forEach((row, index) => {
            if (index >= startIndex && index < endIndex) {
                row.style.display = 'table-row';
            } else {
                row.style.display = 'none';
            }
        });
    }


    // Function to update pagination buttons and page numbers
    function updatePagination() {
        pageNumbers.textContent =
            `Page ${currentPage} of ${totalPages}`;
        prevButton.disabled = currentPage === 1;
        nextButton.disabled = currentPage === totalPages;
        pageLinks.forEach((link) => {
            const page = parseInt(link.getAttribute('data-page'));
            link.classList.toggle('active', page === currentPage);
        });
    }

 
    prevButton.addEventListener('click', () => {
        if (currentPage > 1) {
            currentPage--;
            displayPage(currentPage);
            updatePagination();
        }
    });

   
    nextButton.addEventListener('click', () => {
        if (currentPage < totalPages) {
            currentPage++;
            displayPage(currentPage);
            updatePagination();
        }
    });

    // Event listener for page number buttons
    pageLinks.forEach((link) => {
        link.addEventListener('click', (e) => {
            e.preventDefault();
            const page = parseInt(link.getAttribute('data-page'));
            if (page !== currentPage) {
                currentPage = page;
                displayPage(currentPage);
                updatePagination();
            }
        });
    });

    displayPage(currentPage);
    updatePagination();
}


