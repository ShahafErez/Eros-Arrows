import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { map } from 'rxjs';
import { AccountService } from '../_services/account.service';

export const authGuard: CanActivateFn = (route, state) => {
  const accountService = inject(AccountService);
  const toaster = inject(ToastrService);

  return accountService.currentUser$.pipe(
    map((user) => {
      if (user) return true;
      else {
        toaster.error('Please login to see this page');
        return false;
      }
    })
  );
};
