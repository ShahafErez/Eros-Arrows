import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, of, take } from 'rxjs';
import { Member } from 'src/app/_models/member';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';
import {
  getPaginatedResult,
  getPaginationHeaders,
} from '../_serviecs/PaginationHelper';
import { UserParams } from './../_models/userParams';
import { AccountService } from './account.service';

@Injectable({
  providedIn: 'root',
})
export class MembersService {
  baseUrlUsers = environment.apiUrl + '/users';
  baseUrlLikes = environment.apiUrl + '/likes';
  members: Member[] = [];
  memberCache = new Map();
  user: User | undefined;
  userParams: UserParams | undefined;

  constructor(
    private http: HttpClient,
    private accountService: AccountService
  ) {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: (user) => {
        if (user) {
          this.userParams = new UserParams(user);
          this.user = user;
        }
      },
    });
  }

  getUserParams() {
    return this.userParams;
  }

  setUserParams(params: UserParams) {
    this.userParams = params;
  }

  resetUserParams() {
    if (this.user) {
      this.userParams = new UserParams(this.user);
      return this.userParams;
    }
    return;
  }

  getMembers(userParams: UserParams) {
    const hashMapKey = Object.values(userParams).join('-');
    // checking if the response is already in the hashmap
    const hashMapValue = this.memberCache.get(hashMapKey);
    if (hashMapValue) return of(hashMapValue);

    let queryParams = getPaginationHeaders(
      userParams.pageNumber,
      userParams.pageSize
    );

    queryParams = queryParams.append('minAge', userParams.minAge);
    queryParams = queryParams.append('maxAge', userParams.maxAge);
    queryParams = queryParams.append('gender', userParams.gender);
    queryParams = queryParams.append('orderBy', userParams.orderBy);

    return getPaginatedResult<Member[]>(
      this.baseUrlUsers,
      queryParams,
      this.http
    ).pipe(
      map((response) => {
        this.memberCache.set(hashMapKey, response);
        return response;
      })
    );
  }

  getMember(username: string) {
    const member = [...this.memberCache.values()]
      .reduce((array, element) => array.concat(element.result), [])
      .find((member: Member) => member.userName === username);

    if (member) return of(member);
    return this.http.get<Member>(`${this.baseUrlUsers}/${username}`);
  }

  updateMember(member: Member) {
    return this.http.put(this.baseUrlUsers, member).pipe(
      map(() => {
        const index = this.members.indexOf(member);
        this.members[index] = { ...this.members[index], ...member };
      })
    );
  }

  setMainPhoto(photoId: number) {
    return this.http.put(`${this.baseUrlUsers}/set-main-photo/${photoId}`, {});
  }

  deletePhoto(photoId: number) {
    return this.http.delete(`${this.baseUrlUsers}/delete-photo/${photoId}`);
  }

  addLike(username: string) {
    return this.http.post(`${this.baseUrlLikes}/${username}`, {});
  }

  getLikes(predicate: string, pageNumber: number, pageSize: number) {
    let params = getPaginationHeaders(pageNumber, pageSize);
    params = params.append('predicate', predicate);
    return getPaginatedResult<Member[]>(this.baseUrlLikes, params, this.http);
  }
}
