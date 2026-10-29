// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener("DOMContentLoaded", () => {
	const showToast = (message) => {
		const toastArea = document.getElementById("toastArea");
		const toast = document.createElement("div");
		toast.className = "toast show border-0 shadow-sm";
		toast.setAttribute("role", "status");
		toast.innerHTML = `<div class="toast-body"><i class="bi bi-check-circle-fill text-success me-2"></i>${message}</div>`;
		toastArea.appendChild(toast);
		window.setTimeout(() => toast.remove(), 3200);
	};

	document.querySelectorAll(".slot:not(.disabled)").forEach((slot) => {
		slot.addEventListener("click", () => {
			document.querySelectorAll(".slot").forEach((item) => item.classList.remove("active"));
			slot.classList.add("active");
		});
	});

	document.querySelectorAll(".demo-form").forEach((form) => {
		form.addEventListener("submit", (event) => {
			event.preventDefault();
			if (!form.checkValidity()) {
				form.classList.add("was-validated");
				return;
			}
			showToast("Thanks. Your request has been noted in this demo.");
			form.reset();
		});
	});

	document.querySelectorAll("[data-toast]").forEach((action) => {
		action.addEventListener("click", (event) => {
			if (action.tagName === "A" && action.getAttribute("href")?.startsWith("#")) return;
			event.preventDefault();
			showToast(action.dataset.toast);
		});
	});

	document.getElementById("continueBooking")?.addEventListener("click", () => {
		showToast("Great choice. Your next step is to select a doctor.");
	});

	document.querySelectorAll(".navbar-nav .nav-link").forEach((link) => {
		link.addEventListener("click", () => {
			const menu = document.getElementById("mainNav");
			if (menu.classList.contains("show")) bootstrap.Collapse.getOrCreateInstance(menu).hide();
		});
	});
});
