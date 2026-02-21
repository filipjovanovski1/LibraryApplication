(function () {
    const pageSize = 9;

    function normalize(value) {
        return (value || "").toString().trim().toLowerCase();
    }

    document.addEventListener("DOMContentLoaded", function () {
        const searchInput = document.getElementById("booksSearchInput");
        const grid = document.getElementById("booksGrid");
        const prevButton = document.getElementById("booksPrevPage");
        const nextButton = document.getElementById("booksNextPage");
        const pageInfo = document.getElementById("booksPageInfo");
        const paginationWrapper = document.getElementById("booksPaginationWrapper");
        const noResults = document.getElementById("booksNoResults");

        if (!grid || !searchInput || !prevButton || !nextButton || !pageInfo || !paginationWrapper || !noResults) {
            return;
        }

        const items = Array.from(grid.querySelectorAll(".book-item"));
        let filteredItems = items;
        let currentPage = 1;

        function render() {
            const totalPages = Math.max(1, Math.ceil(filteredItems.length / pageSize));
            if (currentPage > totalPages) {
                currentPage = totalPages;
            }

            const start = (currentPage - 1) * pageSize;
            const end = start + pageSize;

            items.forEach((item) => item.classList.add("d-none"));
            filteredItems.slice(start, end).forEach((item) => item.classList.remove("d-none"));

            pageInfo.textContent = `Page ${currentPage} of ${totalPages}`;
            prevButton.disabled = currentPage === 1;
            nextButton.disabled = currentPage === totalPages;
            paginationWrapper.classList.toggle("d-none", filteredItems.length <= pageSize);
            noResults.classList.toggle("d-none", filteredItems.length > 0);
        }

        function applySearch() {
            const query = normalize(searchInput.value);

            filteredItems = items.filter((item) => {
                const haystack = normalize(item.getAttribute("data-search"));
                return haystack.includes(query);
            });

            currentPage = 1;
            render();
        }

        searchInput.addEventListener("input", applySearch);

        prevButton.addEventListener("click", function () {
            if (currentPage > 1) {
                currentPage -= 1;
                render();
            }
        });

        nextButton.addEventListener("click", function () {
            const totalPages = Math.max(1, Math.ceil(filteredItems.length / pageSize));
            if (currentPage < totalPages) {
                currentPage += 1;
                render();
            }
        });

        render();
    });
})();