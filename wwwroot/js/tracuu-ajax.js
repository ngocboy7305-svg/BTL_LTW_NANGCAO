const traCuuState = {
    tuKhoa: "",
    trangThaiHoSo: "",
    ketQuaXetTuyen: "",
    currentPage: 1,
    pageSize: 5,
    totalPages: 1
};

let traCuuDebounceTimer = null;

function loadTraCuuWithXHR(page = 1) {
    traCuuState.currentPage = page;

    const params = new URLSearchParams({
        tuKhoa: traCuuState.tuKhoa,
        trangThaiHoSo: traCuuState.trangThaiHoSo,
        ketQuaXetTuyen: traCuuState.ketQuaXetTuyen,
        page: traCuuState.currentPage,
        pageSize: traCuuState.pageSize
    });

    const xhr = new XMLHttpRequest();
    xhr.open("GET", `/TraCuu/GetListJson?${params.toString()}`, true);
    xhr.setRequestHeader("X-Requested-With", "XMLHttpRequest");
    xhr.setRequestHeader("Accept", "application/json");

    xhr.onreadystatechange = function () {
        if (xhr.readyState === 4 && xhr.status === 200) {
            const res = JSON.parse(xhr.responseText);
            if (res.success) {
                traCuuState.currentPage = res.currentPage;
                traCuuState.totalPages = res.totalPages;
                renderTraCuuTableDOM(res.data);
                renderLichSuTraCuuDOM(res.lichSu);
                renderTraCuuPaginationDOM(res.currentPage, res.totalPages, res.totalItems);
            }
        }
    };

    xhr.send();
}

async function searchTraCuuWithFetch(resetPage = true) {
    if (resetPage) traCuuState.currentPage = 1;

    traCuuState.tuKhoa = document.getElementById("txtSearchTraCuu")?.value.trim() || "";
    traCuuState.trangThaiHoSo = document.getElementById("selTrangThaiHoSo")?.value || "";
    traCuuState.ketQuaXetTuyen = document.getElementById("selKetQuaXetTuyen")?.value || "";
    traCuuState.pageSize = parseInt(document.getElementById("selPageSizeTraCuu")?.value, 10) || 5;

    const params = new URLSearchParams({
        tuKhoa: traCuuState.tuKhoa,
        trangThaiHoSo: traCuuState.trangThaiHoSo,
        ketQuaXetTuyen: traCuuState.ketQuaXetTuyen,
        page: traCuuState.currentPage,
        pageSize: traCuuState.pageSize
    });

    try {
        const response = await fetch(`/TraCuu/GetListJson?${params.toString()}`, {
            method: "GET",
            headers: { "Accept": "application/json", "X-Requested-With": "XMLHttpRequest" }
        });

        if (!response.ok) throw new Error(`HTTP ${response.status}`);
        const res = await response.json();
        if (res.success) {
            traCuuState.currentPage = res.currentPage;
            traCuuState.totalPages = res.totalPages;
            renderTraCuuTableDOM(res.data);
            renderLichSuTraCuuDOM(res.lichSu);
            renderTraCuuPaginationDOM(res.currentPage, res.totalPages, res.totalItems);
        }
    } catch (err) {
        console.error(err);
    }
}

function renderTraCuuTableDOM(items) {
    const tbody = document.getElementById("traCuuTableBody");
    if (!tbody) return;

    if (!items || items.length === 0) {
        tbody.innerHTML = `<tr><td colspan="7" class="text-center py-4 text-muted">Không tìm thấy hồ sơ phù hợp.</td></tr>`;
        return;
    }

    let html = "";
    items.forEach(h => {
        let badge = `<span class="badge bg-warning-subtle text-warning-emphasis border">Chờ duyệt</span>`;
        if (h.trangThaiHoSo === "Hợp lệ") badge = `<span class="badge bg-success-subtle text-success border border-success-subtle">Hợp lệ</span>`;
        else if (h.trangThaiHoSo === "Không hợp lệ") badge = `<span class="badge bg-danger-subtle text-danger border border-danger-subtle">Không hợp lệ</span>`;

        html += `
            <tr>
                <td class="ps-3 fw-bold text-primary">${h.maHoSo}</td>
                <td>
                    <div class="fw-semibold text-dark">${h.hoTen}</div>
                    <div class="small text-muted">CCCD: ${h.soCCCD}</div>
                </td>
                <td class="small">KV: ${h.khuVucUuTien} | +${Number(h.diemCongUuTien).toFixed(2)}đ</td>
                <td class="text-center">${badge}</td>
                <td class="small text-secondary">${h.ghiChuDuyet}</td>
                <td><strong class="${h.daTrungTuyen ? 'text-success' : 'text-secondary'}">${h.nganhTrungTuyen}</strong></td>
                <td class="text-end pe-3"><span class="badge bg-light text-dark border">${h.soNguyenVong} NV</span></td>
            </tr>`;
    });

    tbody.innerHTML = html;
}

function renderLichSuTraCuuDOM(logs) {
    const tbody = document.getElementById("lichSuTraCuuBody");
    if (!tbody || !logs) return;

    let html = "";
    logs.forEach(l => {
        html += `
            <tr id="row-ls-${l.maLichSuId}">
                <td class="ps-3 small fw-semibold">${l.loaiThaoTac}</td>
                <td class="small text-secondary">${l.noiDungChiTiet}</td>
                <td class="small">${l.nguoiThucHien}</td>
                <td class="small text-muted">${l.thoiGian}</td>
                <td class="text-end pe-3">
                    <button class="btn btn-outline-danger btn-sm py-0 px-2 btn-delete-ls" data-id="${l.maLichSuId}">Xóa</button>
                </td>
            </tr>`;
    });
    tbody.innerHTML = html;
}

function renderTraCuuPaginationDOM(currentPage, totalPages, totalItems) {
    const info = document.getElementById("traCuuPagingInfo");
    const ul = document.getElementById("traCuuPagination");

    if (info) info.innerHTML = `Tổng: <strong>${totalItems}</strong> hồ sơ — Trang <strong>${currentPage}</strong> / <strong>${totalPages || 1}</strong>`;
    if (!ul) return;
    if (totalPages <= 1) { ul.innerHTML = ""; return; }

    let html = `<li class="page-item ${currentPage <= 1 ? "disabled" : ""}"><a class="page-link" href="#" onclick="loadTraCuuWithXHR(${currentPage - 1}); return false;">Trước</a></li>`;
    for (let p = 1; p <= totalPages; p++) {
        html += `<li class="page-item ${p === currentPage ? "active" : ""}"><a class="page-link" href="#" onclick="loadTraCuuWithXHR(${p}); return false;">${p}</a></li>`;
    }
    html += `<li class="page-item ${currentPage >= totalPages ? "disabled" : ""}"><a class="page-link" href="#" onclick="loadTraCuuWithXHR(${currentPage + 1}); return false;">Sau</a></li>`;
    ul.innerHTML = html;
}

$(document).ready(function () {
    loadTraCuuWithXHR(1);

    $("#txtSearchTraCuu").on("input", function () {
        clearTimeout(traCuuDebounceTimer);
        traCuuDebounceTimer = setTimeout(() => searchTraCuuWithFetch(true), 300);
    });

    $("#selTrangThaiHoSo, #selKetQuaXetTuyen, #selPageSizeTraCuu").on("change", () => searchTraCuuWithFetch(true));

    $("#formCreateGhiChuAjax").on("submit", function (e) {
        e.preventDefault();
        const payload = {
            MaHoSoId: parseInt($("#inpMaHoSoId").val(), 10),
            LoaiThaoTac: $("#inpLoaiThaoTac").val(),
            NoiDungChiTiet: $("#inpNoiDungGhiChu").val().trim()
        };

        $.ajax({
            url: "/TraCuu/CreateGhiChuAjax",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify(payload),
            success: function (res) {
                if (res.success) {
                    $("#inpNoiDungGhiChu").val("");
                    bootstrap.Modal.getInstance(document.getElementById("modalCreateGhiChu")).hide();
                    loadTraCuuWithXHR(traCuuState.currentPage);
                }
            }
        });
    });

    $(document).on("click", ".btn-delete-ls", function () {
        const id = $(this).data("id");
        if (!confirm("Bạn có muốn xóa nhật ký này?")) return;

        $.ajax({
            url: "/TraCuu/DeleteLichSuAjax",
            type: "POST",
            dataType: "json",
            data: { id: id },
            success: function (res) {
                if (res.success) {
                    $(`#row-ls-${id}`).fadeOut(300, function () { $(this).remove(); });
                }
            }
        });
    });
});