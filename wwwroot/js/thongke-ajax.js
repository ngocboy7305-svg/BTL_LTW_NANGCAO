const thongKeState = {
    tuKhoa: "",
    khoa: "",
    sortOrder: "ma_asc",
    currentPage: 1,
    pageSize: 5,
    totalPages: 1
};

let thongKeDebounceTimer = null;

function loadThongKeWithXHR(page = 1) {
    thongKeState.currentPage = page;

    const params = new URLSearchParams({
        tuKhoa: thongKeState.tuKhoa,
        khoa: thongKeState.khoa,
        sortOrder: thongKeState.sortOrder,
        page: thongKeState.currentPage,
        pageSize: thongKeState.pageSize
    });

    const xhr = new XMLHttpRequest();
    xhr.open("GET", "/Admin/ThongKe/GetListJson?" + params.toString(), true);
    xhr.setRequestHeader("X-Requested-With", "XMLHttpRequest");
    xhr.setRequestHeader("Accept", "application/json");

    xhr.onreadystatechange = function () {
        if (xhr.readyState === 4 && xhr.status === 200) {
            const res = JSON.parse(xhr.responseText);
            if (res.success) {
                thongKeState.currentPage = res.currentPage;
                thongKeState.totalPages = res.totalPages;

                renderKpiDOM(res.kpi);
                renderThongKeTableDOM(res.data);
                renderBaoCaoLogsDOM(res.baoCaoLogs);
                renderThongKePaginationDOM(res.currentPage, res.totalPages, res.totalItems);
            }
        }
    };

    xhr.send();
}

async function searchThongKeWithFetch(resetPage = true) {
    if (resetPage) thongKeState.currentPage = 1;

    thongKeState.tuKhoa = document.getElementById("txtSearchThongKe")?.value.trim() || "";
    thongKeState.khoa = document.getElementById("selKhoaThongKe")?.value || "";
    thongKeState.sortOrder = document.getElementById("selSortThongKe")?.value || "ma_asc";
    thongKeState.pageSize = parseInt(document.getElementById("selPageSizeThongKe")?.value, 10) || 5;

    const params = new URLSearchParams({
        tuKhoa: thongKeState.tuKhoa,
        khoa: thongKeState.khoa,
        sortOrder: thongKeState.sortOrder,
        page: thongKeState.currentPage,
        pageSize: thongKeState.pageSize
    });

    try {
        const response = await fetch("/Admin/ThongKe/GetListJson?" + params.toString(), {
            method: "GET",
            headers: { "Accept": "application/json", "X-Requested-With": "XMLHttpRequest" }
        });

        if (!response.ok) throw new Error("HTTP " + response.status);

        const res = await response.json();
        if (res.success) {
            thongKeState.currentPage = res.currentPage;
            thongKeState.totalPages = res.totalPages;

            renderKpiDOM(res.kpi);
            renderThongKeTableDOM(res.data);
            renderBaoCaoLogsDOM(res.baoCaoLogs);
            renderThongKePaginationDOM(res.currentPage, res.totalPages, res.totalItems);
        }
    } catch (err) {
        console.error("Lỗi Fetch:", err);
    }
}

function renderKpiDOM(kpi) {
    if (!kpi) return;
    document.getElementById("kpiTongHoSo").textContent = kpi.tongHoSo;
    document.getElementById("kpiHoSoHopLe").textContent = kpi.hoSoHopLe;
    document.getElementById("kpiHoSoChoDuyet").textContent = kpi.hoSoChoDuyet;
    document.getElementById("kpiHoSoKhongHopLe").textContent = kpi.hoSoKhongHopLe;
    document.getElementById("kpiTongNguyenVong").textContent = kpi.tongNguyenVong;
    document.getElementById("kpiTongThiSinh").textContent = kpi.tongThiSinh;
    document.getElementById("kpiTongTrungTuyen").textContent = kpi.tongTrungTuyen;
    document.getElementById("kpiTongThongBao").textContent = kpi.tongThongBao;
}

function renderThongKeTableDOM(items) {
    const tbody = document.getElementById("thongKeTableBody");
    if (!tbody) return;

    if (!items || items.length === 0) {
        tbody.innerHTML = '<tr><td colspan="9" class="text-center py-4 text-muted">Không có ngành đào tạo nào khớp bộ lọc.</td></tr>';
        return;
    }

    let html = "";
    items.forEach(n => {
        const diemChuanBadge = n.diemChuan !== null
            ? '<span class="fw-bold text-danger">' + Number(n.diemChuan).toFixed(2) + '</span>'
            : '<span class="text-muted small">Chưa công bố</span>';

        html += '<tr >' +
            '<td class="ps-3 fw-bold text-primary">' + n.maNganh + '</td>' +
            '<td class="fw-semibold text-dark">' + n.tenNganh + '</td>' +
            '<td><span class="badge bg-light text-dark border">' + n.tenKhoa + '</span></td>' +
            '<td class="text-center fw-bold">' + n.chiTieu + '</td>' +
            '<td class="text-center fw-semibold text-primary">' + n.tongNguyenVong + '</td>' +
            '<td class="text-center">' + n.soNV1 + '</td>' +
            '<td class="text-center"><span class="badge bg-info-subtle text-info-emphasis border">1 : ' + n.tyLeChoi + '</span></td>' +
            '<td class="text-center small">Sàn: ' + Number(n.diemSan).toFixed(2) + ' | Chuẩn: ' + diemChuanBadge + '</td>' +
            '<td class="text-center pe-3"><span class="fw-bold text-success">' + n.soTrungTuyen + '</span> / ' + n.chiTieu + ' (' + n.tyLeLapDay + '%)</td>' +
            '</tr>';
    });

    tbody.innerHTML = html;
}

function renderBaoCaoLogsDOM(logs) {
    const tbody = document.getElementById("baoCaoLogBody");
    if (!tbody) return;

    if (!logs || logs.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" class="text-center py-3 text-muted small">Chưa có biên bản báo cáo nào.</td></tr>';
        return;
    }

    let html = "";
    logs.forEach(l => {
        html += '<tr id="row-baocao-' + l.maLichSuId + '">' +
            '<td class="ps-3 fw-bold text-primary small">#' + l.maLichSuId + '</td>' +
            '<td class="small fw-semibold">' + l.loaiThaoTac + '</td>' +
            '<td class="small text-secondary">' + l.noiDungChiTiet + '</td>' +
            '<td class="small fw-semibold">' + l.nguoiThucHien + '</td>' +
            '<td class="small text-muted">' + l.thoiGian + '</td>' +
            '<td class="text-end pe-3"><button type="button" class="btn btn-outline-danger btn-sm py-0 px-2 btn-delete-baocao" data-id="' + l.maLichSuId + '">Xóa</button></td>' +
            '</tr>';
    });

    tbody.innerHTML = html;
}

function renderThongKePaginationDOM(currentPage, totalPages, totalItems) {
    const infoEl = document.getElementById("thongKePagingInfo");
    const ulEl = document.getElementById("thongKePagination");

    if (infoEl) infoEl.innerHTML = "Thống kê <strong>" + totalItems + "</strong> ngành — Trang <strong>" + currentPage + "</strong> / <strong>" + (totalPages || 1) + "</strong>";
    if (!ulEl) return;
    if (totalPages <= 1) { ulEl.innerHTML = ""; return; }

    let html = '<li class="page-item ' + (currentPage <= 1 ? "disabled" : "") + '"><a class="page-link" href="#" onclick="loadThongKeWithXHR(' + (currentPage - 1) + '); return false;">Trước</a></li>';
    for (let p = 1; p <= totalPages; p++) {
        html += '<li class="page-item ' + (p === currentPage ? "active" : "") + '"><a class="page-link" href="#" onclick="loadThongKeWithXHR(' + p + '); return false;">' + p + '</a></li>';
    }
    html += '<li class="page-item ' + (currentPage >= totalPages ? "disabled" : "") + '"><a class="page-link" href="#" onclick="loadThongKeWithXHR(' + (currentPage + 1) + '); return false;">Sau</a></li>';
    ulEl.innerHTML = html;
}

$(document).ready(function () {
    loadThongKeWithXHR(1);

    $("#txtSearchThongKe").on("input", function () {
        clearTimeout(thongKeDebounceTimer);
        thongKeDebounceTimer = setTimeout(function () { searchThongKeWithFetch(true); }, 300);
    });

    $("#selKhoaThongKe, #selSortThongKe, #selPageSizeThongKe").on("change", function () {
        searchThongKeWithFetch(true);
    });

    $("#formCreateBaoCaoAjax").on("submit", function (e) {
        e.preventDefault();

        const payload = {
            LoaiThaoTac: $("#inpLoaiBaoCao").val().trim(),
            NguoiThucHien: $("#inpNguoiLapBaoCao").val().trim(),
            NoiDungChiTiet: $("#inpNoiDungBaoCao").val().trim()
        };

        $.ajax({
            url: "/Admin/ThongKe/CreateBaoCaoAjax",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify(payload),
            success: function (res) {
                if (res.success) {
                    $("#inpNoiDungBaoCao").val("");
                    bootstrap.Modal.getInstance(document.getElementById("modalCreateBaoCao")).hide();
                    showThongKeAlert(res.message, "success");
                    loadThongKeWithXHR(thongKeState.currentPage);
                }
            }
        });
    });

    $(document).on("click", ".btn-delete-baocao", function () {
        const id = $(this).data("id");
        if (!confirm("Bạn có chắc chắn muốn xóa biên bản báo cáo này không?")) return;

        $.ajax({
            url: "/Admin/ThongKe/DeleteBaoCaoAjax",
            type: "POST",
            dataType: "json",
            data: { id: id },
            success: function (res) {
                if (res.success) {
                    $("#row-baocao-" + id).fadeOut(300, function () { $(this).remove(); });
                    showThongKeAlert(res.message, "success");
                }
            }
        });
    });
});

function showThongKeAlert(message, type) {
    const box = document.getElementById("thongKeAlertBox");
    if (!box) return;
    box.className = "alert alert-" + (type || "success") + " py-2 small shadow-sm";
    box.textContent = message;
    box.classList.remove("d-none");
    setTimeout(function () { box.classList.add("d-none"); }, 3500);
}