using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zxw.Framework.NetCore.Attributes;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.Models;
using Zxw.Framework.Website.Controllers.Filters;
using Zxw.Framework.Website.IRepositories;
using Zxw.Framework.Website.Models;
using Zxw.Framework.Website.ViewModels;

namespace Zxw.Framework.Website.Controllers
{
    [ControllerDescription(Name = "�˵�����")]
    public class SysMenuController : BaseController
    {
        private ISysMenuRepository menuRepository;
        
        public SysMenuController(ISysMenuRepository menuRepository)
        {
            this.menuRepository = menuRepository ?? throw new ArgumentNullException(nameof(menuRepository));
        }

        #region Views
        [ActionDescription(Name = "�˵��б�")]
        public IActionResult Index()
        {
            return View();
        }
        [ActionDescription(Name = "�½��˵�")]
        public IActionResult Create()
        {
            return View();
        }
        [ActionDescription(Name = "�༭�˵�")]
        public IActionResult Edit(string id)
        {
            return View(menuRepository.GetSingle(id));
        }

        #endregion

        #region Methods

        [AjaxRequestOnly, HttpGet, ActionDescription(Description = "Ajax��ȡ�˵��б�", Name = "��ȡ�˵��б�")]
        public Task<IActionResult> GetMenus()
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var rows = menuRepository.GetHomeMenusByTreeView(m=>m.Activable && m.Visiable && string.IsNullOrEmpty(m.ParentId)).OrderBy(m=>m.SortIndex).ToList();
                return Json(ExcutedResult.SuccessResult(rows));
            });
        }

        [AjaxRequestOnly, HttpGet, ActionDescription(Name = "��ȡ�˵���", Description = "Ajax��ȡ�˵���")]
        public Task<IActionResult> GetTreeMenus(string parentId = null)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var nodes = menuRepository.GetMenusByTreeView(m => m.Activable && string.IsNullOrEmpty(m.ParentId))
                    .OrderBy(m => m.SortIndex).Select(m => GetTreeMenus(m, parentId)).ToList();
                var rows = new[]
                {
                    new
                    {
                        text = " ���ڵ�",
                        icon = "fas fa-boxes",
                        tags = "0",
                        nodes,
                        state = new
                        {
                            selected = string.IsNullOrEmpty(parentId)
                        }
                    }
                };
                return Json(ExcutedResult.SuccessResult(rows));
            });
        }

        private object GetTreeMenus(SysMenuViewModel viewModel, string parentId = null)
        {
            if (viewModel.Children.Any())
            {
                return new
                {
                    text = " "+viewModel.MenuName,
                    icon = viewModel.MenuIcon,
                    tags = viewModel.Id.ToString(),
                    nodes = viewModel.Children.Select(x=>GetTreeMenus(x, parentId)),
                    state = new
                    {
                        expanded = false,
                        selected = viewModel.Id == parentId
                    }
                };
            }
            return new 
            {
                text = " "+viewModel.MenuName,
                icon = viewModel.MenuIcon,
                tags = viewModel.Id.ToString(),
                state = new
                {
                    selected = viewModel.Id == parentId
                }
            };
        }

        [AjaxRequestOnly, HttpGet, ActionDescription(Name = "��ȡ�˵��б�", Description = "Ajax��ҳ��ȡ�˵��б�")]
        public Task<IActionResult> GetMenusByPaged(int pageSize, int pageIndex, string keyword)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                Expression<Func<SysMenu, bool>> filter = m=>true;
                if(!string.IsNullOrEmpty(keyword))
                    filter = filter.And(m=>m.Identity.Contains(keyword));
                var total = menuRepository.CountAsync(filter).Result;
                var rows = menuRepository.GetByPagination(filter, pageSize, pageIndex, true,
                    m => m.Id).ToList();
                return Json(PaginationResult.PagedResult(rows, total, pageSize, pageIndex));
            });
        }
        /// <summary>
        /// �½�
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        [AjaxRequestOnly,HttpPost,ValidateAntiForgeryToken, ActionDescription(Name = "�½��˵�", Description = "Ajax�½��˵�")]
        public Task<IActionResult> Add(SysMenu menu)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                if(!ModelState.IsValid)
                    return Json(ExcutedResult.FailedResult("������֤ʧ��"));
                menuRepository.AddAsync(menu);
                return Json(ExcutedResult.SuccessResult());
            });
        }
        /// <summary>
        /// �༭
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        [AjaxRequestOnly, HttpPost, ActionDescription(Name = "�༭�˵�", Description = "Ajax�༭�˵�")]
        public Task<IActionResult> Edit(SysMenu menu)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                if (!ModelState.IsValid)
                    return Json(ExcutedResult.FailedResult("������֤ʧ��"));
                menuRepository.Edit(menu);
                return Json(ExcutedResult.SuccessResult());
            });
        }
        /// <summary>
        /// ɾ��
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AjaxRequestOnly, ActionDescription(Name = "ɾ���˵�", Description = "Ajaxɾ���˵�")]
        public Task<IActionResult> Delete(string id)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                menuRepository.Delete(id);
                return Json(ExcutedResult.SuccessResult("�ɹ�ɾ��һ�����ݡ�"));
            });
        }

        /// <summary>
        /// ��ͣ��
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AjaxRequestOnly, ActionDescription(Name = "��/ͣ�ò˵�")]
        public Task<IActionResult> Active(string id)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var entity = menuRepository.GetSingle(id);
                entity.Activable = !entity.Activable;
                menuRepository.Update(entity, "Activable");
                return Json(ExcutedResult.SuccessResult(entity.Activable?"OK���ѳɹ����á�":"OK���ѳɹ�ͣ��"));
            });
        }
        /// <summary>
        /// �Ƿ������˵�����ʾ
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AjaxRequestOnly, ActionDescription(Name = "��ʾ/���ز˵�")]
        public Task<IActionResult> Visualize(string id)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var entity = menuRepository.GetSingle(id);
                entity.Visiable = !entity.Visiable;
                menuRepository.Update(entity, "Visiable");
                return Json(ExcutedResult.SuccessResult("�����ɹ�����ˢ�µ�ǰ��ҳ�������½���ϵͳ��"));
            });
        }

        #endregion
	}
}