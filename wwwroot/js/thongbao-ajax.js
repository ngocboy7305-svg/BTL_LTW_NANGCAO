const thongBaoState = {
    tuKhoa: "",
    trangThaiXem: "",
    sortOrder: "newest",
    currentPage: 1,
    pageSize: 5,
    totalPages: 1
};

let thongBaoDebounceTimer = null;

function loadThongBaoWithXHR(page = 1) {
    thongBaoState.currentPage = page;

    const params = new URLSearchParams({
        tuKhoa: thongBaoState.tuKhoa,
        trangThaiXem: thongBaoState.trangThaiXem,
        sortOrder: thongBaoState.sortOrder,
        page: thongBaoState.currentPage,
        pageSize: thongBaoState.pageSize
    });

    const xhr = new XMLHttpRequest();
    xhr.open("GET", `/ThongBao/GetListJson?${params.toString()}`, true);
    xhr.setRequestHeader("X-Requested-With", "XMLHttpRequest");
    xhr.setRequestHeader("Accept", "application/json");

    xhr.onreadystatechange = function () {
        if (xhr.readyState === 4 && xhr.status === 200) {
            try {
                const response = JSON.parse(xhr.responseText);
                if (response.success) {
                    thongBaoState.currentPage = response.currentPage;
                    thongBaoState.totalPages = response.totalPages;

                    renderThongBaoTableDOM(response.data);
                    renderThongBaoPaginationDOM(response.currentPage, response.totalPages, response.totalItems);
                    updateUnreadBadge(response.unreadCount);
                }
            } catch (err) {
                console.error("Lỗi parse JSON:", err);
            }
        }
    };

    xhr.send();
}

async function searchThongBaoWithFetch(resetPage = true) {
    if (resetPage) thongBaoState.currentPage = 1;

    const txtSearch = document.getElementById("txtSearchThongBao");
    const selTrangThai = document.getElementById("selTrangThaiXem");
    const selSort = document.getElementById("selSortThongBao");
    const selPageSize = document.getElementById("selPageSizeThongBao");

    if (txtSearch) thongBaoState.tuKhoa = txtSearch.value.trim();
    if (selTrangThai) thongBaoState.trangThaiXem = selTrangThai.value;
    if (selSort) thongBaoState.sortOrder = selSort.value;
    if (selPageSize) thongBaoState.pageSize = parseInt(selPageSize.value, 10) || 5;

    const params = new URLSearchParams({
        tuKhoa: thongBaoState.tuKhoa,
        trangThaiXem: thongBaoState.trangThaiXem,
        sortOrder: thongBaoState.sortOrder,
        page: thongBaoState.currentPage,
        pageSize: thongBaoState.pageSize
    });

    try {
        const response = await fetch(`/ThongBao/GetListJson?${params.toString()}`, {
            method: "GET",
            headers: { "Accept": "application/json", "X-Requested-With": "XMLHttpRequest" }
        });

        if (!response.ok) throw new Error(`HTTP ${response.status}`);

        const result = await response.json();
        if (result.success) {
            thongBaoState.currentPage = result.currentPage;
            thongBaoState.totalPages = result.totalPages;

            renderThongBaoTableDOM(result.data);
            renderThongBaoPaginationDOM(result.currentPage, result.totalPages, result.totalItems);
            updateUnreadBadge(result.unreadCount);
        }
    } catch (error) {
        console.error("Lỗi Fetch:", error);
    }
}

function renderThongBaoTableDOM(items) {
    const tbody = document.getElementById("thongBaoTableBody");
    if (!tbody) return;

    if (!items || items.length === 0) {
        tbody.innerHTML = `<tr><td colspan="7" class="text-center py-4 text-muted">Không tìm thấy thông báo nào.</td></tr>`;
        return;
    }

    let html = "";
    items.forEach(item => {
        const statusBadge = item.daXem
            ? `<span class="badge bg-secondary-subtle text-secondary border">Đã xem</span>`
            : `<span class="badge bg-success-subtle text-success border border-success-subtle fw-bold">● Mới</span>`;

        html += `
            <tr id="row-thongbao-${item.maThongBaoId}">
                <td class="ps-3 fw-bold text-primary">#${item.maThongBaoId}</td>
                <td>
                    <div class="fw-semibold text-dark">${escapeHtml(item.tenNguoiNhan)}</div>
                    <div class="small text-muted">CCCD: ${escapeHtml(item.soCCCD)}</div>
                </td>
                <td class="fw-semibold text-dark">${escapeHtml(item.tieuDe)}</td>
                <td class="small text-secondary">${escapeHtml(item.noiDung)}</td>
                <td class="text-center">${statusBadge}</td>
                <td class="text-center small text-muted">${escapeHtml(item.ngayGui)}</td>
                <td class="text-end pe-3">
                    <button type="button" class="btn btn-outline-primary btn-sm me-1 btn-toggle-read"
                            data-id="${item.maThongBaoId}">
                        ${item.daXem ? "Chưa đọc" : "Đã đọc"}
                    </button>
                    <button type="button" class="btn btn-outline-danger btn-sm btn-delete-thongbao"
                            data-id="${item.maThongBaoId}"
                            data-tieude="${escapeHtml(item.tieuDe)}">
                        Xóa
                    </button>
                </td>
            </tr>`;
    });

    tbody.innerHTML = html;
}

function renderThongBaoPaginationDOM(currentPage, totalPages, totalItems) {
    const infoEl = document.getElementById("thongBaoPagingInfo");
    const ulEl = document.getElementById("thongBaoPagination");

    if (infoEl) infoEl.innerHTML = `Tổng số: <strong>${totalItems}</strong> thông báo — Trang <strong>${currentPage}</strong> / <strong>${totalPages || 1}</strong>`;
    if (!ulEl) return;
    if (totalPages <= 1) { ulEl.innerHTML = ""; return; }

    let html = `<li class="page-item ${currentPage <= 1 ? "disabled" : ""}"><a class="page-link" href="#" onclick="changeThongBaoPage(${currentPage - 1}); return false;">Trước</a></li>`;
    for (let p = 1; p <= totalPages; p++) {
        html += `<li class="page-item ${p === currentPage ? "active" : ""}"><a class="page-link" href="#" onclick="changeThongBaoPage(${p}); return false;">${p}</a></li>`;
    }
    html += `<li class="page-item ${currentPage >= totalPages ? "disabled" : ""}"><a class="page-link" href="#" onclick="changeThongBaoPage(${currentPage + 1}); return false;">Sau</a></li>`;
    ulEl.innerHTML = html;
}

function changeThongBaoPage(page) {
    if (page < 1 || (thongBaoState.totalPages > 0 && page > thongBaoState.totalPages)) return;
    loadThongBaoWithXHR(page);
}

function updateUnreadBadge(unreadCount) {
    const badge = document.getElementById("badgeUnreadCount");
    if (badge) badge.textContent = `${unreadCount || 0} thông báo chưa đọc`;
}

$(document).ready(function () {
    loadThongBaoWithXHR(1);

    $("#txtSearchThongBao").on("input", function () {
        clearTimeout(thongBaoDebounceTimer);
        thongBaoDebounceTimer = setTimeout(() => searchThongBaoWithFetch(true), 300);
    });

    $("#selTrangThaiXem, #selSortThongBao, #selPageSizeThongBao").on("change", () => searchThongBaoWithFetch(true));

    $("#formCreateThongBaoAjax").on("submit", function (e) {
        e.preventDefault();
        const payload = {
            MaNguoiNhan: parseInt($("#inpMaNguoiNhan").val(), 10) || 0,
            TieuDe: $("#inpTieuDeThongBao").val().trim(),
            NoiDung: $("#inpNoiDungThongBao").val().trim()
        };

        $.ajax({
            url: "/ThongBao/CreateAjax",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify(payload),
            success: function (res) {
                if (res.success) {
                    $("#formCreateThongBaoAjax")[0].reset();
                    bootstrap.Modal.getInstance(document.getElementById("modalCreateThongBao")).hide();
                    showThongBaoAlert(res.message, "success");
                    loadThongBaoWithXHR(1);
                }
            }
        });
    });

    $(document).on("click", ".btn-delete-thongbao", function () {
        const id = $(this).data("id");
        const tieuDe = $(this).data("tieude");

        if (!confirm(`Bạn có chắc chắn muốn xóa thông báo "${tieuDe}" không?`)) return;

        $.ajax({
            url: "/ThongBao/DeleteAjax",
            type: "POST",
            dataType: "json",
            data: { id: id },
            success: function (res) {
                if (res.success) {
                    $("#row-thongbao-" + id).fadeOut(300, function () {
                        $(this).remove();
                        loadThongBaoWithXHR(thongBaoState.currentPage);
                    });
                    showThongBaoAlert(res.message, "success");
                }
            }
        });
    });

    $(document).on("click", ".btn-toggle-read", function () {
        const id = $(this).data("id");
        $.ajax({
            url: "/ThongBao/ToggleDaXemAjax",
            type: "POST",
            dataType: "json",
            data: { id: id },
            success: function (res) {
                if (res.success) {
                    showThongBaoAlert(res.message, "info");
                    loadThongBaoWithXHR(thongBaoState.currentPage);
                }
            }
        });
    });
});

function showThongBaoAlert(message, type = "success") {
    const box = document.getElementById("thongBaoAlertBox");
    if (!box) return;
    box.className = `alert alert-${type} py-2 small shadow-sm`;
    box.textContent = message;
    box.classList.remove("d-none");
    setTimeout(() => box.classList.add("d-none"), 3500);
}

function escapeHtml(str) {
    if (!str) return "";
    return String(str).replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
}