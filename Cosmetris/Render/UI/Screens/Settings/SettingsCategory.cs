/*
 * SettingsCategory.cs is part of Cosmetris.
 *
 * Copyright (c) 2023 CKProductions, https://ckproductions.dev/
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Linq;
using Cosmetris.Render.UI.Controls;

namespace Cosmetris.Render.UI.Screens.Settings;

public class SettingsCategory
{
    public SettingsCategory(string categoryName)
    {
        CategoryName = categoryName;
        Pages = new List<SettingsCategoryPage>();
        CurrentPage = 0;
        SetCurrentPage(0);
    }

    public string CategoryName { get; set; }
    public int CurrentPage { get; set; }
    public SettingsCategoryPage ActivePage { get; set; }
    public int TotalPages { get; set; }

    public List<SettingsCategoryPage> Pages { get; }

    public bool ChangePage(ref Panel panel, bool increase)
    {
        if (increase)
        {
            if (CurrentPage >= TotalPages) return false;
            panel.RemoveControlTaggedContains("prefCat");

            SetCurrentPage(CurrentPage + 1);

            foreach (var control in ActivePage.CategoryOptions) panel.AddControl(control);

            return true;
        }

        if (CurrentPage <= 0) return false;
        {
            panel.RemoveControlTaggedContains("prefCat");

            SetCurrentPage(CurrentPage - 1);

            foreach (var control in ActivePage.CategoryOptions) panel.AddControl(control);

            return true;
        }
    }

    public void SetCurrentPage(int currentPage)
    {
        CurrentPage = currentPage;

        SettingsCategoryPage catPage = null;

        //Make sure the page exists
        if (CurrentPage >= Pages.Count)
        {
            catPage = new SettingsCategoryPage();

            Pages.Add(catPage);
        }

        ActivePage = catPage ?? Pages.ElementAt(CurrentPage);
    }

    public void AddControlToPage(Control control, int page)
    {
        if (page >= Pages.Count) Pages.Add(new SettingsCategoryPage());

        control.Tag += "prefCat";

        Pages.ElementAt(page).AddControl(control);
    }

    public void Layout(ref Panel panel)
    {
        panel.RemoveControlTaggedContains("prefCat");

        SetCurrentPage(CurrentPage);

        foreach (var control in ActivePage.CategoryOptions) panel.AddControl(control);
    }

    public int TruePageCount()
    {
        return Pages.Count;
    }

    public void RemoveControlFromPage(Control control, int page)
    {
        if (page >= Pages.Count) return;

        Pages.ElementAt(page).RemoveControl(control);
    }
}